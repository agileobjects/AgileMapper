namespace AgileObjects.AgileMapper.DataSources.Finders
{
    using System.Collections.Generic;

    internal interface IDataSourceFinder
    {
        IEnumerable<IDataSource> FindFor(DataSourceFindContext context);
    }
}