namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Members;
    using Members.Dictionaries;

    internal class ExistingMemberValueOrDefaultDataSource : DataSourceBase
    {
        public ExistingMemberValueOrDefaultDataSource(IMemberMapperData mapperData)
            : base(mapperData.SourceMember, GetValue(mapperData), mapperData)
        {
        }

        private static Expression GetValue(IMemberMapperData mapperData)
        {
            if (mapperData.TargetMember.IsEnumerable)
            {
                return FallbackToCollection(mapperData)
                    ? mapperData.GetFallbackCollectionValue()
                    : mapperData.GetTargetMemberDefault();
            }

            if (mapperData.TargetMember.IsReadable && !mapperData.UseMemberInitialisations())
            {
                return mapperData.GetTargetMemberAccess();
            }

            return mapperData.GetTargetMemberDefault();
        }

        private static bool FallbackToCollection(IBasicMapperData mapperData)
        {
            if (mapperData.TargetMember.IsDictionary)
            {
                return true;
            }

            if (!(mapperData.TargetMember is DictionaryTargetMember dictionaryTargetMember))
            {
                return true;
            }

            return dictionaryTargetMember.HasEnumerableEntries;
        }
    }
}