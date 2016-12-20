namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class DictionaryDataSourceFactory : IMaptimeDataSourceFactory
    {
        public bool IsFor(IMemberMapperData mapperData) => mapperData.HasUseableSourceDictionary();

        public IDataSource Create(IChildMemberMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var sourceMember = GetSourceMember(mapperData);

            if (mapperData.TargetMember.IsSimple)
            {
                return new DictionaryEntryDataSource(sourceMember, mapperData);
            }

            return new DictionaryDataSource(sourceMember, mapperData);
        }

        private static DictionarySourceMember GetSourceMember(IMemberMapperData mapperData)
        {
            if (!mapperData.TargetMember.IsRecursive)
            {
                return new DictionarySourceMember(mapperData);
            }

            var parentMapperData = mapperData.Parent;

            while (!parentMapperData.IsRoot)
            {
                if (parentMapperData.TargetMember.LeafMember == mapperData.TargetMember.LeafMember)
                {
                    break;
                }

                parentMapperData = parentMapperData.Parent;
            }

            return (DictionarySourceMember)parentMapperData.SourceMember;
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