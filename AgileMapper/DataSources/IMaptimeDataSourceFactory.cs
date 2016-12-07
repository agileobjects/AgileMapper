namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using Members;

    internal interface IMaptimeDataSourceFactory
    {
        bool IsFor(IMemberMapperData mapperData);

        IEnumerable<IDataSource> Create(IChildMemberMappingData mappingData);
    }
}