namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal static class InlineMappingFactory
    {
        public static Expression GetRuntimeTypedMapping(
            ObjectMapperData mapperData,
            Expression sourceValue,
            Type targetType)
        {
            var runtimeMapperData = mapperData.WithTypes(sourceValue.Type, targetType);

            var targetValue = mapperData.TargetMember.IsReadable
                ? mapperData.TargetObject.GetConversionTo(targetType)
                : Expression.Default(targetType);

            if (runtimeMapperData.IsRoot)
            {
                return GetRootMapping(runtimeMapperData, mapperData, sourceValue, targetValue);
            }

            if (mapperData.TargetMember.LeafMember.MemberType == MemberType.EnumerableElement)
            {
                return GetElementMapping(
                    sourceValue,
                    targetValue,
                    Expression.Property(mapperData.EnumerableIndex, "Value"),
                    runtimeMapperData,
                    mapperData,
                    PopulateMappingDataMapperData,
                    ResetMappingDataMapperData);
            }

            return GetChildMapping(
                runtimeMapperData.SourceMember,
                sourceValue,
                targetValue,
                mapperData.EnumerableIndex,
                mapperData.DataSourceIndex,
                runtimeMapperData,
                mapperData,
                PopulateMappingDataMapperData,
                ResetMappingDataMapperData);
        }

        private static void PopulateMappingDataMapperData(IObjectMappingData mappingData, ObjectMapperData runtimeMapperData)
        {
            mappingData.MapperData = runtimeMapperData;
            mappingData.MapperData.MappingData.MapperData = runtimeMapperData;
        }

        private static void ResetMappingDataMapperData(IObjectMappingData mappingData, ObjectMapperData preInlineMapperData)
        {
            preInlineMapperData.MappingData.MapperData = preInlineMapperData;
        }

        private static Expression GetRootMapping(
            ObjectMapperData runtimeMapperData,
            ObjectMapperData preInlineMapperData,
            Expression sourceValue,
            Expression targetValue)
        {
            var rootMappingData = ObjectMappingDataFactory.ForRoot(runtimeMapperData);

            PopulateMappingDataMapperData(rootMappingData, runtimeMapperData);

            var rootMapper = runtimeMapperData.IsPartOfDerivedTypeMapping
                ? rootMappingData.Mapper
                : runtimeMapperData.MapperContext.ObjectMapperFactory.CreateRoot(rootMappingData);

            var inlineMappingBlock = GetInlineMappingBlock(
                rootMapper,
                MappingDataFactory.ForRootMethod,
                sourceValue,
                targetValue,
                Expression.Property(preInlineMapperData.MappingDataObject, "MappingContext"));

            ResetMappingDataMapperData(rootMappingData, preInlineMapperData);

            return inlineMappingBlock;
        }

        public static Expression GetChildMapping(int dataSourceIndex, IMemberMapperData childMapperData)
        {
            var relativeMember = childMapperData.SourceMember.RelativeTo(childMapperData.SourceMember);
            var sourceMemberAccess = relativeMember.GetQualifiedAccess(childMapperData.SourceObject);

            return GetChildMapping(
                relativeMember,
                sourceMemberAccess,
                dataSourceIndex,
                childMapperData);
        }

        public static Expression GetChildMapping(
            IQualifiedMember sourceMember,
            Expression sourceMemberAccess,
            int dataSourceIndex,
            IMemberMapperData childMapperData)
        {
            var targetMemberAccess = childMapperData.GetTargetMemberAccess();

            return GetChildMapping(
                sourceMember,
                sourceMemberAccess,
                targetMemberAccess,
                childMapperData.Parent.EnumerableIndex,
                dataSourceIndex,
                childMapperData,
                childMapperData.Parent,
                (mappingData, mapperData) => { },
                (mappingData, mapperData) => { });
        }

        private static Expression GetChildMapping<TMapperData>(
            IQualifiedMember sourceMember,
            Expression sourceValue,
            Expression targetValue,
            Expression enumerableIndex,
            int dataSourceIndex,
            TMapperData childMapperData,
            ObjectMapperData preInlineMapperData,
            Action<IObjectMappingData, TMapperData> preMapperCreationCallback,
            Action<IObjectMappingData, ObjectMapperData> postMapperCreationCallback)
            where TMapperData : IMemberMapperData
        {
            var childMappingData = ObjectMappingDataFactory.ForChild(
                sourceMember,
                childMapperData.TargetMember,
                dataSourceIndex,
                childMapperData.Parent.MappingData);

            if (childMappingData.MapperKey.MappingTypes.RuntimeTypesNeeded)
            {
                return preInlineMapperData.GetMapCall(sourceValue, childMapperData.TargetMember, dataSourceIndex);
            }

            var createMethodCallArguments = new[]
            {
                sourceValue,
                targetValue,
                enumerableIndex,
                Expression.Constant(childMapperData.TargetMember.RegistrationName),
                Expression.Constant(dataSourceIndex),
                preInlineMapperData.MappingDataObject
            };

            preMapperCreationCallback.Invoke(childMappingData, childMapperData);

            if (childMapperData.TargetMemberReferencesRecursionRoot())
            {
                var mapperFuncCall = GetMapperFuncCallFor(childMappingData.MapperData, createMethodCallArguments);

                postMapperCreationCallback.Invoke(childMappingData, preInlineMapperData);

                return mapperFuncCall;
            }

            var childMapper = childMappingData.Mapper;

            var inlineMappingBlock = GetInlineMappingBlock(
                childMapper,
                MappingDataFactory.ForChildMethod,
                createMethodCallArguments);

            postMapperCreationCallback.Invoke(childMappingData, preInlineMapperData);

            return inlineMappingBlock;
        }

        private static Expression GetMapperFuncCallFor(
            ObjectMapperData childMapperData,
            Expression[] createMethodCallArguments)
        {
            var mapperFuncVariable = childMapperData.FindRequiredMapperFuncVariable();

            var createInlineMappingDataCall = GetCreateMappingDataCall(
                MappingDataFactory.ForChildMethod,
                childMapperData,
                createMethodCallArguments);

            var mapperFuncCall = Expression.Invoke(mapperFuncVariable, createInlineMappingDataCall);

            return mapperFuncCall;
        }

        public static Expression GetElementMapping(IMemberMapperData elementMapperData)
        {
            return GetElementMapping(
                elementMapperData.SourceObject,
                elementMapperData.TargetObject,
                Parameters.EnumerableIndex,
                elementMapperData,
                elementMapperData.Parent,
                (mappingData, mapperData) => { },
                (mappingData, mapperData) => { });
        }

        private static Expression GetElementMapping<TMapperData>(
            Expression sourceObject,
            Expression targetObject,
            Expression enumerableIndex,
            TMapperData elementMapperData,
            ObjectMapperData preInlineMapperData,
            Action<IObjectMappingData, TMapperData> preMapperCreationCallback,
            Action<IObjectMappingData, ObjectMapperData> postMapperCreationCallback)
            where TMapperData : IMemberMapperData
        {
            var elementMappingData = ObjectMappingDataFactory.ForElement(
                elementMapperData.SourceMember,
                elementMapperData.TargetMember,
                elementMapperData.Parent.MappingData);

            if (elementMappingData.MapperKey.MappingTypes.RuntimeTypesNeeded)
            {
                return preInlineMapperData.GetMapCall(sourceObject, targetObject);
            }

            preMapperCreationCallback.Invoke(elementMappingData, elementMapperData);

            var elementMapper = elementMappingData.Mapper;

            var inlineMappingBlock = GetInlineMappingBlock(
                elementMapper,
                MappingDataFactory.ForElementMethod,
                sourceObject,
                targetObject,
                enumerableIndex,
                preInlineMapperData.MappingDataObject);

            postMapperCreationCallback.Invoke(elementMappingData, preInlineMapperData);

            return inlineMappingBlock;
        }

        private static Expression GetInlineMappingBlock(
            IObjectMapper childMapper,
            MethodInfo createMethod,
            params Expression[] createMethodCallArguments)
        {
            var mappingExpression = MappingExpression.For(childMapper);

            if (!mappingExpression.IsSuccessful)
            {
                return childMapper.MappingExpression;
            }

            var childMapperData = childMapper.MapperData;
            var inlineMappingDataVariable = childMapperData.MappingDataObject;

            var createInlineMappingDataCall = GetCreateMappingDataCall(
                createMethod,
                childMapperData,
                createMethodCallArguments);

            var inlineMappingDataAssignment = Expression
                .Assign(inlineMappingDataVariable, createInlineMappingDataCall);

            var updatedMappingExpression = mappingExpression.GetUpdatedMappingExpression(inlineMappingDataAssignment);

            return updatedMappingExpression;
        }

        private static Expression GetCreateMappingDataCall(
            MethodInfo createMethod,
            IBasicMapperData childMapperData,
            Expression[] createMethodCallArguments)
        {
            var inlineMappingTypes = new[] { childMapperData.SourceType, childMapperData.TargetType };

            return Expression.Call(
                createMethod.MakeGenericMethod(inlineMappingTypes),
                createMethodCallArguments);
        }

        #region Helper Class

        private class MappingExpression
        {
            private static readonly MappingExpression _unableToMap = new MappingExpression();

            private readonly TryExpression _mappingTryCatch;
            private readonly Func<BlockExpression, Expression> _finalMappingBlockFactory;

            private MappingExpression()
            {
            }

            private MappingExpression(
                TryExpression mappingTryCatch,
                Func<BlockExpression, Expression> finalMappingBlockFactory)
            {
                _mappingTryCatch = mappingTryCatch;
                _finalMappingBlockFactory = finalMappingBlockFactory;
                IsSuccessful = true;
            }

            #region Factory Method

            public static MappingExpression For(IObjectMapper mapper)
            {
                if (mapper.MappingExpression.NodeType == ExpressionType.Try)
                {
                    return new MappingExpression((TryExpression)mapper.MappingExpression, b => b);
                }

                var blockExpression = (BlockExpression)mapper.MappingExpression;

                var mappingTryCatch = blockExpression.Expressions.Last() as TryExpression;

                if (mappingTryCatch == null)
                {
                    return _unableToMap;
                }

                return new MappingExpression(
                    mappingTryCatch,
                    updatedTryCatch =>
                    {
                        var blockExpressions = blockExpression
                            .Expressions
                            .Take(blockExpression.Expressions.Count - 1)
                            .ToList();

                        blockExpressions.Add(updatedTryCatch);

                        return Expression.Block(blockExpression.Variables, blockExpressions);
                    });
            }

            #endregion

            public bool IsSuccessful { get; }

            public Expression GetUpdatedMappingExpression(BinaryExpression mappingDataAssignment)
            {
                var updatedTryCatch = _mappingTryCatch.Update(
                    Expression.Block(mappingDataAssignment, _mappingTryCatch.Body),
                    _mappingTryCatch.Handlers,
                    _mappingTryCatch.Finally,
                    _mappingTryCatch.Fault);

                var mappingDataVariable = (ParameterExpression)mappingDataAssignment.Left;
                var mappingBlock = Expression.Block(new[] { mappingDataVariable }, updatedTryCatch);
                var finalMappingBlock = _finalMappingBlockFactory.Invoke(mappingBlock);

                return finalMappingBlock;
            }
        }

        #endregion
    }
}