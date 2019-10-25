namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;
    using Members;

    internal delegate IEnumerable<IDataSource> MaptimeDataSourceFactory(
        IChildMemberMappingData mappingData);
}
