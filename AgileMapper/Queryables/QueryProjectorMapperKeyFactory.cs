namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq;
    using ObjectPopulation.MapperKeys;

    internal static class QueryProjectorMapperKeyFactory
    {
        public static ObjectMapperKeyBase Create(IEntryPointMappingContext context)
        {
            var mappingTypes = context.MappingTypes;
            var providerType = context.GetSource<IQueryable>().Provider.GetType();
            var mapperContext = context.MapperContext;

            return new QueryProjectorKey(mappingTypes, providerType, mapperContext)
            {
                MappingData = null,
                // TODO: MappingContext = context
            };
        }
    }
}