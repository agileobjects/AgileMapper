namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;

    internal delegate IEnumerable<IDataSource> DataSourceFactory(DataSourceFindContext context);
}