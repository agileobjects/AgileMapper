namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal static class ChildMappingFactory
    {
        public static Expression GetChildMapping(
            IQualifiedMember sourceMember,
            Expression sourceMemberAccess,
            int dataSourceIndex,
            IMemberMapperData mapperData)
        {
            var inlineMapping = GetInlineChildMappingOrNull(
                sourceMember,
                sourceMemberAccess,
                dataSourceIndex,
                mapperData);

            return inlineMapping ?? mapperData.GetMapCall(sourceMemberAccess, dataSourceIndex);
        }

        private static Expression GetInlineChildMappingOrNull(
            IQualifiedMember sourceMember,
            Expression sourceMemberAccess,
            int dataSourceIndex,
            IMemberMapperData mapperData)
        {
            if (!mapperData.CanInlineMappingFor(mapperData.TargetMember))
            {
                return null;
            }

            var childMappingData = ObjectMappingDataFactory.ForChild(
                sourceMember,
                mapperData.TargetMember,
                dataSourceIndex,
                mapperData.Parent.MappingData);

            var childMapper = childMappingData.CreateMapper();

            if (childMapper.MapperData.RequiresChildMapping || childMapper.MapperData.RequiresElementMapping)
            {
                return null;
            }

            var inlineMappingBlock = GetInlineMappingBlock(
                sourceMemberAccess,
                dataSourceIndex,
                mapperData,
                childMapper);

            childMapper.MapperData.Parent.MappingInlinedFor(mapperData.TargetMember);

            return inlineMappingBlock;
        }

        private static Expression GetInlineMappingBlock(
            Expression sourceMemberAccess,
            int dataSourceIndex,
            IMemberMapperData mapperData,
            IObjectMapper childMapper)
        {
            var childMapperData = childMapper.MapperData;
            var inlineMappingTypes = new[] { childMapperData.SourceType, childMapperData.TargetType };
            var inlineMappingDataType = typeof(InlineChildMappingData<,>).MakeGenericType(inlineMappingTypes);
            var inlineMappingDataVariableName = childMapperData.TargetType.GetVariableNameInCamelCase() + "Data";
            var inlineMappingDataVariable = Expression.Variable(inlineMappingDataType, inlineMappingDataVariableName);

            var createInlineMappingDataMethod = MappingDataFactory
                .ForChildMethod
                .MakeGenericMethod(inlineMappingTypes);

            var targetMemberAccess = mapperData.TargetMember.GetAccess(mapperData.Parent.InstanceVariable);

            var createInlineMappingDataCall = Expression.Call(
                createInlineMappingDataMethod,
                sourceMemberAccess,
                targetMemberAccess,
                mapperData.EnumerableIndex,
                Expression.Constant(mapperData.TargetMember.RegistrationName),
                Expression.Constant(dataSourceIndex),
                mapperData.Parameter);

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