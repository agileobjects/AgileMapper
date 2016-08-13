namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class ComplexTypeMappingDataSource : DataSourceBase
    {
        public ComplexTypeMappingDataSource(
            IQualifiedMember bestMatchingSourceMember,
            int dataSourceIndex,
            IMemberMapperCreationData data)
            : base(
                  bestMatchingSourceMember ?? data.SourceMember,
                  MapCall.For(bestMatchingSourceMember, dataSourceIndex, data))
        {
        }
    }
}