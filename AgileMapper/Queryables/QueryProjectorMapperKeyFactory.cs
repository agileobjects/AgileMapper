namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq;
    using ObjectPopulation;
    using ObjectPopulation.MapperKeys;

    internal struct QueryProjectorMapperKeyFactory : IRootMapperKeyFactory
    {
        public ObjectMapperKeyBase CreateRootKeyFor(IObjectMappingData mappingData)
        {
            var providerType = mappingData.GetSource<IQueryable>().Provider.GetType();

            return new QueryProjectorKey(mappingData.MappingTypes, providerType, mappingData.MappingContext.MapperContext)
            {
                MappingData = mappingData
            };
        }
    }
}