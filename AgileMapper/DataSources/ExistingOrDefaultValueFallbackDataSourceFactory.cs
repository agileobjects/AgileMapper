namespace AgileObjects.AgileMapper.DataSources
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;
    using Members.Dictionaries;

    internal struct ExistingOrDefaultValueFallbackDataSourceFactory : IFallbackDataSourceFactory
    {
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
                    return FallbackToCollection(mapperData)
                        ? mapperData.GetFallbackCollectionValue()
                        : mapperData.GetTargetMemberDefault();
                }

                if (mapperData.TargetMember.IsReadable && !mapperData.UseMemberInitialisations())
                {
                    return mapperData.TargetMember.IsSimple
                        ? Constants.EmptyExpression
                        : mapperData.GetTargetMemberAccess();
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

            public override bool IsFallback => true;
        }
    }
} 