namespace AgileObjects.AgileMapper.Api.Configuration
{
    public class PreEventConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal PreEventConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        //public CallbackSpecifier<object, object> MappingInstances => null;
    }
}