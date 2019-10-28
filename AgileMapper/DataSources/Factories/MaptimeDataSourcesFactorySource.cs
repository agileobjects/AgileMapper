namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System;
    using System.Collections.Generic;
    using Members;

    internal delegate bool MaptimeDataSourcesFactorySource(
        IMemberMapperData mapperData,
        out Func<IChildMemberMappingData, IEnumerable<IDataSource>> maptimeDataSourceFactory);
}