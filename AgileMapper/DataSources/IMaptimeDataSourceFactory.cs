namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using Members;

    internal interface IMaptimeDataSourceFactory
    {
        bool IsFor(IBasicMapperData mapperData);

        IEnumerable<IMaptimeDataSource> Create(IChildMemberMappingData mappingData);
    }
}