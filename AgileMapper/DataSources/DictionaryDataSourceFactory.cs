namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class DictionaryDataSourceFactory : IMaptimeDataSourceFactory
    {
        public bool IsFor(IMemberMapperData mapperData) => mapperData.HasUseableSourceDictionary();

        public IDataSource Create(IChildMemberMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var sourceMember = new DictionarySourceMember(mapperData);

            if (mapperData.TargetMember.IsSimple)
            {
                return new DictionaryEntryDataSource(sourceMember, mapperData);
            }

            return new DictionaryDataSource(sourceMember, mapperData);
        }

        private class DictionaryDataSource : DataSourceBase
        {
            public DictionaryDataSource(IQualifiedMember sourceMember, IMemberMapperData mapperData)
                : base(
                      sourceMember,
                      sourceMember.GetQualifiedAccess(mapperData.SourceObject))
            {
            }
        }
    }
}