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
        public static Expression GetChildMapping(
            IQualifiedMember sourceMember,
            Expression sourceMemberAccess,
            int dataSourceIndex,
            IMemberMapperData childMapperData)
        {
            var inlineMapping = GetInlineChildMappingOrNull(
                sourceMember,
                sourceMemberAccess,
                dataSourceIndex,
                childMapperData);

            return inlineMapping ?? childMapperData.GetMapCall(sourceMemberAccess, dataSourceIndex);
        }

        private static Expression GetInlineChildMappingOrNull(
            IQualifiedMember sourceMember,
            Expression sourceMemberAccess,
            int dataSourceIndex,
            IMemberMapperData childMapperData)
        {
            if (!childMapperData.CanInlineMappingFor(childMapperData.TargetMember))
            {
                return null;
            }

            var childMappingData = ObjectMappingDataFactory.ForChild(
                sourceMember,
                childMapperData.TargetMember,
                dataSourceIndex,
                childMapperData.Parent.MappingData);

            var childMapper = childMappingData.CreateMapper();

            var inlineMappingBlock = GetInlineMappingBlockOrNull(
                childMapperData,
                childMapper,
                MappingDataFactory.ForChildMethod,
                md => new[]
                {
                    sourceMemberAccess,
                    md.TargetMember.GetAccess(md.Parent.InstanceVariable),
                    md.EnumerableIndex,
                    Expression.Constant(md.TargetMember.RegistrationName),
                    Expression.Constant(dataSourceIndex),
                    md.Parameter
                });

            if (inlineMappingBlock == null)
            {
                return null;
            }

            childMapper.MapperData.Parent.MappingInlinedFor(childMapperData.TargetMember);

            return inlineMappingBlock;
        }

        public static Expression GetElementMapping(
            Expression sourceObject,
            Expression existingObject,
            ObjectMapperData enumerableMapperData)
        {
            var inlineMapping = GetInlineElementMappingOrNull(
                sourceObject,
                existingObject,
                enumerableMapperData);

            return inlineMapping ?? enumerableMapperData.GetMapCall(sourceObject, existingObject);
        }

        private static Expression GetInlineElementMappingOrNull(
            Expression sourceObject,
            Expression existingObject,
            ObjectMapperData enumerableMapperData)
        {
            if (!enumerableMapperData.CanInlineMappingFor(enumerableMapperData.TargetElementMember))
            {
                return null;
            }

            var elementMappingData = ObjectMappingDataFactory.ForElement(enumerableMapperData.MappingData);

            var elementMapper = elementMappingData.CreateMapper();

            var inlineMappingBlock = GetInlineMappingBlockOrNull(
                enumerableMapperData,
                elementMapper,
                MappingDataFactory.ForElementMethod,
                md => new[]
                {
                    sourceObject,
                    existingObject,
                    Parameters.EnumerableIndex,
                    md.Parameter
                });

            if (inlineMappingBlock == null)
            {
                return null;
            }

            enumerableMapperData.ElementMappingInlined();

            return inlineMappingBlock;
        }

        private static Expression GetInlineMappingBlockOrNull(
            IMemberMapperData mapperData,
            IObjectMapper childMapper,
            MethodInfo createMethod,
            Func<IMemberMapperData, Expression[]> createMethodCallArgumentsFactory)
        {
            if (childMapper.MapperData.RequiresChildMapping || childMapper.MapperData.RequiresElementMapping)
            {
                return null;
            }

            var childMapperData = childMapper.MapperData;
            var inlineMappingTypes = new[] { childMapperData.SourceType, childMapperData.TargetType };
            var createInlineMappingDataMethod = createMethod.MakeGenericMethod(inlineMappingTypes);
            var inlineMappingDataType = createInlineMappingDataMethod.ReturnType;
            var inlineMappingDataVariableName = childMapperData.TargetType.GetVariableNameInCamelCase() + "Data";
            var inlineMappingDataVariable = Expression.Variable(inlineMappingDataType, inlineMappingDataVariableName);

            var methodCallArguments = createMethodCallArgumentsFactory.Invoke(mapperData);
            var createInlineMappingDataCall = Expression.Call(createInlineMappingDataMethod, methodCallArguments);

            var inlineMappingDataAssignment = Expression.Assign(inlineMappingDataVariable, createInlineMappingDataCall);

            var replacementsByTarget = new ExpressionReplacementDictionary
            {
                [childMapperData.CreatedObject] = Expression.Property(inlineMappingDataVariable, "CreatedObject"),
                [childMapperData.Parameter] = inlineMappingDataVariable
            };

            var mappingTryCatch = (TryExpression)childMapper.MappingLambda.Body.Replace(replacementsByTarget);

            mappingTryCatch = mappingTryCatch.Update(
                Expression.Block(inlineMappingDataAssignment, mappingTryCatch.Body),
                mappingTryCatch.Handlers,
                mappingTryCatch.Finally,
                mappingTryCatch.Fault);

            return Expression.Block(new[] { inlineMappingDataVariable }, mappingTryCatch);
        }
    }
}