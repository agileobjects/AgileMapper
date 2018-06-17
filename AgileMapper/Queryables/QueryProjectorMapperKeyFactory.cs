namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq;
    using ObjectPopulation;
    using ObjectPopulation.MapperKeys;

    internal struct QueryProjectorMapperKeyFactory : IRootMapperKeyFactory
    {
        public ObjectMapperKeyBase CreateRootKeyFor<TSource, TTarget>(ObjectMappingData<TSource, TTarget> mappingData)
        {
            var providerType = ((IQueryable)mappingData.Source).Provider.GetType();

            return new QueryProjectorKey(mappingData.MappingTypes, providerType, mappingData.MapperContext)
            {
                MappingData = mappingData
            };
        }
    }
}