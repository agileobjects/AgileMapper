namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;

    internal interface IDataSourceFactory
    {
        IEnumerable<IDataSource> CreateFor(DataSourceFindContext context);
    }
}