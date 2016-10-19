namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal class ComplexTypeMappingDataSource : DataSourceBase
    {
        public ComplexTypeMappingDataSource(
            IQualifiedMember bestMatchingSourceMember,
            int dataSourceIndex,
            IMemberMapperData mapperData)
            : base(
                  bestMatchingSourceMember ?? mapperData.SourceMember,
                  GetMapCall(bestMatchingSourceMember ?? mapperData.SourceMember, dataSourceIndex, mapperData))
        {
        }

        private static Expression GetMapCall(
            IQualifiedMember sourceMember,
            int dataSourceIndex,
            IMemberMapperData mapperData)
        {
            var relativeMember = sourceMember.RelativeTo(mapperData.SourceMember);
            var relativeMemberAccess = relativeMember.GetQualifiedAccess(mapperData.SourceObject);
            var inlineMapping = GetInlineMappingOrNull(relativeMember, relativeMemberAccess, dataSourceIndex, mapperData);

            return inlineMapping ?? mapperData.GetMapCall(relativeMemberAccess, dataSourceIndex);
        }

        private static Expression GetInlineMappingOrNull(
            IQualifiedMember sourceMember,
            Expression relativeSourceMemberAccess,
            int dataSourceIndex,
            IMemberMapperData mapperData)
        {
            if (!mapperData.TargetMember.Type.IsSealed())
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

            var inlineMappingTypes = new[] { childMapper.MapperData.SourceType, childMapper.MapperData.TargetType };
            var inlineMappingDataType = typeof(InlineChildMappingData<,>).MakeGenericType(inlineMappingTypes);
            var inlineMappingDataVariableName = childMapper.MapperData.TargetType.GetVariableNameInCamelCase() + "Data";
            var inlineMappingDataVariable = Expression.Variable(inlineMappingDataType, inlineMappingDataVariableName);

            var createInlineMappingDataMethod = MappingDataFactory
                .ForChildMethod
                .MakeGenericMethod(inlineMappingTypes);

            var targetMemberAccess = mapperData.TargetMember.GetAccess(mapperData.Parent.InstanceVariable);

            var createInlineMappingDataCall = Expression.Call(
                createInlineMappingDataMethod,
                relativeSourceMemberAccess,
                targetMemberAccess,
                mapperData.EnumerableIndex,
                Expression.Constant(mapperData.TargetMember.RegistrationName),
                Expression.Constant(dataSourceIndex),
                mapperData.Parameter);

            var inlineMappingDataAssignment = Expression.Assign(inlineMappingDataVariable, createInlineMappingDataCall);

            var replacementsByTarget = new ExpressionReplacementDictionary
            {
                [childMapper.MapperData.CreatedObject] = Expression.Property(inlineMappingDataVariable, "CreatedObject"),
                [childMapper.MapperData.Parameter] = inlineMappingDataVariable
            };

            var mappingTryCatch = (TryExpression)childMapper.MappingLambda.Body.Replace(replacementsByTarget);

            mappingTryCatch = mappingTryCatch.Update(
                Expression.Block(inlineMappingDataAssignment, mappingTryCatch.Body),
                mappingTryCatch.Handlers,
                mappingTryCatch.Finally,
                mappingTryCatch.Fault);

            var inlineMappingBlock = Expression.Block(new[] { inlineMappingDataVariable }, mappingTryCatch);

            childMapper.MapperData.Parent.MappingInlinedFor(mapperData.TargetMember);

            return inlineMappingBlock;
        }
    }
}