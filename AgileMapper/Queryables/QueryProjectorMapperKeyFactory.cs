namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq;
    using ObjectPopulation.MapperKeys;

    internal static class QueryProjectorMapperKeyFactory
    {
        public static ObjectMapperKeyBase Create(MappingExecutionContextBase2 context)
        {
            var mappingTypes = context.MappingTypes;
            var providerType = ((IQueryable)context.Source).Provider.GetType();
            var mapperContext = context.MapperContext;

            return new QueryProjectorKey(mappingTypes, providerType, mapperContext)
            {
                MappingContext = context
            };
        }
    }
}