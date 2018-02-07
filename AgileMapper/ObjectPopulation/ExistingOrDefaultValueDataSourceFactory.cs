namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using DataSources;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;

    internal class ExistingOrDefaultValueDataSourceFactory : IDataSourceFactory
    {
        public static readonly IDataSourceFactory Instance = new ExistingOrDefaultValueDataSourceFactory();

        public IDataSource Create(IMemberMapperData mapperData)
            => new ExistingMemberValueOrDefaultDataSource(mapperData);

        private class ExistingMemberValueOrDefaultDataSource : DataSourceBase
        {
            public ExistingMemberValueOrDefaultDataSource(IMemberMapperData mapperData)
                : base(mapperData.SourceMember, GetValue(mapperData), mapperData)
            {
            }

            private static Expression GetValue(IMemberMapperData mapperData)
            {
                if (mapperData.TargetMember.IsEnumerable)
                {
                    return FallbackToNull(mapperData)
                        ? mapperData.TargetMember.Type.ToDefaultExpression()
                        : mapperData.GetFallbackCollectionValue();
                }

                if (mapperData.TargetMember.IsReadable && !mapperData.UseMemberInitialisations())
                {
                    return mapperData.GetTargetMemberAccess();
                }

                return mapperData.TargetMember.Type.ToDefaultExpression();
            }

            private static bool FallbackToNull(IBasicMapperData mapperData)
            {
                if (mapperData.TargetMember.IsDictionary)
                {
                    return false;
                }

                if (!(mapperData.TargetMember is DictionaryTargetMember dictionaryTargetMember))
                {
                    return false;
                }

                if (dictionaryTargetMember.HasEnumerableEntries)
                {
                    // TODO: Test coverage - target dictionary with enumerable entries
                    return false;
                }

                return true;
            }
        }
    }
}