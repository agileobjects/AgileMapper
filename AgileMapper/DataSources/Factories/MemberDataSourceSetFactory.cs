namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class MemberDataSourceSetFactory
    {
        private static readonly DataSourcesFactory[] _memberDataSourceFactories =
        {
            ConfiguredDataSourcesFactory.Create,
            MaptimeDataSourcesFactory.Create,
            ConfiguredSimpleTypeFactoryDataSourcesFactory.Create,
            SourceMemberDataSourcesFactory.Create,
            MetaMemberDataSourcesFactory.Create
        };

        public static IDataSourceSet CreateFor(DataSourceFindContext findContext)
        {
            var validDataSources = EnumerateDataSources(findContext).ToArray();

            return DataSourceSet.For(validDataSources, findContext.MemberMapperData);
        }

        private static IEnumerable<IDataSource> EnumerateDataSources(DataSourceFindContext context)
        {
            foreach (var factory in _memberDataSourceFactories)
            {
                foreach (var dataSource in factory.Invoke(context))
                {
                    if (!dataSource.IsValid)
                    {
                        continue;
                    }

                    yield return dataSource;

                    if (!dataSource.IsConditional)
                    {
                        yield break;
                    }

                    ++context.DataSourceIndex;
                }

                if (context.StopFind)
                {
                    yield break;
                }
            }
        }
    }
}