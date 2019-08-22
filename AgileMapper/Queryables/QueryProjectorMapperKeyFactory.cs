namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq;
    using ObjectPopulation;
    using ObjectPopulation.MapperKeys;

    internal static class QueryProjectorMapperKeyFactory
    {
        public static ObjectMapperKeyBase Create(IObjectMappingData mappingData)
        {
            var providerType = mappingData.GetSource<IQueryable>().Provider.GetType();

            return new QueryProjectorKey(mappingData.MappingTypes, providerType, mappingData.MappingContext.MapperContext)
            {
                MappingData = mappingData
            };
        }
    }
}