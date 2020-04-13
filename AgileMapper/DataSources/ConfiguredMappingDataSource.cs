namespace AgileObjects.AgileMapper.DataSources
{
    using Members;
    using ObjectPopulation;

    internal static class ConfiguredMappingDataSource
    {
        public static IDataSource Create(
            IQualifiedMember sourceMember,
            int dataSourceIndex,
            IChildMemberMappingData childMappingData)
        {
            var childMapperData = childMappingData.MapperData;

            var childObjectMappingData = ObjectMappingDataFactory.ForChild(
                sourceMember,
                childMapperData.TargetMember,
                dataSourceIndex,
                childMappingData.Parent);

            var mapping = ConfiguredMappingFactory
                .GetMappingOrNull(childObjectMappingData, out _);

            if (mapping == null)
            {
                return new NullDataSource(Constants.EmptyExpression);
            }

            var childObjectMapperData = childObjectMappingData.MapperData;
            var sourceMemberAccess = sourceMember.GetRelativeQualifiedAccess(childMapperData);
            var targetMemberAccess = childMapperData.GetTargetMemberAccess();

            var mappingValues = new MappingValues(
                sourceMemberAccess,
                targetMemberAccess,
                childMapperData.ElementIndex,
                childMapperData.ElementKey);

            var directAccessMapping = MappingFactory.GetDirectAccessMapping(
                mapping,
                childObjectMapperData,
                mappingValues,
                MappingDataCreationFactory.ForChild(mappingValues, dataSourceIndex, childObjectMapperData));

            var returnLabel = childObjectMapperData
                .GetFinalisedReturnLabel(directAccessMapping, out _);

            return new AdHocDataSource(sourceMember, returnLabel, childMapperData);
        }
    }
}