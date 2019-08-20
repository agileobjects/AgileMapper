namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;

    internal delegate IEnumerable<IDataSource> DataSourcesFactory(DataSourceFindContext context);
}