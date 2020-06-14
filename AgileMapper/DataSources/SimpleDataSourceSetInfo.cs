namespace AgileObjects.AgileMapper.DataSources
{
    using Members;

    internal class SimpleDataSourceSetInfo : IDataSourceSetInfo
    {
        public SimpleDataSourceSetInfo(IMappingContext mappingContext, IMemberMapperData mapperData)
        {
            MappingContext = mappingContext;
            MapperData = mapperData;
        }

        public IMappingContext MappingContext { get; }
        
        public IMemberMapperData MapperData { get; }
    }
}