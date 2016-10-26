namespace AgileObjects.AgileMapper.DataSources
{
    using System;
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
            IMemberMapperData preInlineMapperData,
            Expression sourceValue,
            Expression targetValue)
        {
            var rootMappingData = ObjectMappingDataFactory.ForRoot(runtimeMapperData);
            rootMappingData.MapperData = runtimeMapperData;

            var rootMapper = runtimeMapperData.MapperContext
                .ObjectMapperFactory
                .CreateRoot(rootMappingData);

            var inlineMappingBlock = GetInlineMappingBlock(
                rootMapper,
                MappingDataFactory.ForRootMethod,
                sourceValue,
                targetValue,
                Expression.Property(preInlineMapperData.MappingDataObject, "MappingContext"));

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

            preMapperCreationCallback.Invoke(childMappingData, childMapperData);

            var childMapper = childMappingData.Mapper;

            var inlineMappingBlock = GetInlineMappingBlock(
                childMapper,
                MappingDataFactory.ForChildMethod,
                sourceValue,
                targetValue,
                enumerableIndex,
                Expression.Constant(childMapperData.TargetMember.RegistrationName),
                Expression.Constant(dataSourceIndex),
                preInlineMapperData.MappingDataObject);

            postMapperCreationCallback.Invoke(childMappingData, preInlineMapperData);

            return inlineMappingBlock;
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
            if (childMapper.MappingLambda.Body.NodeType != ExpressionType.Try)
            {
                return childMapper.MappingLambda.Body;
            }

            var childMapperData = childMapper.MapperData;
            var inlineMappingTypes = new[] { childMapperData.SourceType, childMapperData.TargetType };

            var inlineMappingDataVariable = childMapperData.MappingDataObject;

            var createInlineMappingDataCall = Expression.Call(
                createMethod.MakeGenericMethod(inlineMappingTypes),
                createMethodCallArguments);

            var inlineMappingDataAssignment = Expression.Assign(inlineMappingDataVariable, createInlineMappingDataCall);

            var mappingTryCatch = (TryExpression)childMapper.MappingLambda.Body;

            mappingTryCatch = mappingTryCatch.Update(
                Expression.Block(inlineMappingDataAssignment, mappingTryCatch.Body),
                mappingTryCatch.Handlers,
                mappingTryCatch.Finally,
                mappingTryCatch.Fault);

            return Expression.Block(new[] { inlineMappingDataVariable }, mappingTryCatch);
        }
    }
}