namespace AgileObjects.AgileMapper.Api.Configuration
{
    public class InstanceCreationRuleSpecifier : CallbackSpecifier<object>
    {
        private readonly MapperContext _mapperContext;

        internal InstanceCreationRuleSpecifier(MapperContext mapperContext)
            : base(mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public CallbackSpecifier<TTarget> Of<TTarget>()
            where TTarget : class
        {
            return new CallbackSpecifier<TTarget>(_mapperContext);
        }
    }
}