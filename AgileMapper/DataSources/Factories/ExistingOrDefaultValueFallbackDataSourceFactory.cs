namespace AgileObjects.AgileMapper.DataSources.Factories
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;
    using Members.Dictionaries;

    internal static class ExistingOrDefaultValueFallbackDataSourceFactory
    {
        public static IDataSource Create(IMemberMapperData mapperData)
            => new ExistingValueOrDefaultFallbackDataSource(mapperData);

        private class ExistingValueOrDefaultFallbackDataSource : DataSourceBase
        {
            public ExistingValueOrDefaultFallbackDataSource(IMemberMapperData mapperData)
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

            private static bool FallbackToCollection(IQualifiedMemberContext context)
            {
                if (context.TargetMember.IsDictionary)
                {
                    return true;
                }

                if (!(context.TargetMember is DictionaryTargetMember dictionaryTargetMember))
                {
                    return true;
                }

                return dictionaryTargetMember.HasEnumerableEntries;
            }

            public override bool IsFallback => true;
        }
    }
} 