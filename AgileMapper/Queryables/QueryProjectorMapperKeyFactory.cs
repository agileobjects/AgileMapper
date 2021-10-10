namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq;
    using ObjectPopulation.MapperKeys;

    internal static class QueryProjectorMapperKeyFactory
    {
        public static ObjectMapperKeyBase Create(IMapperKeyData data)
        {
            var providerType = ((IQueryable)data.Source).Provider.GetType();
            return new QueryProjectorKey(data, providerType);
        }
    }
}