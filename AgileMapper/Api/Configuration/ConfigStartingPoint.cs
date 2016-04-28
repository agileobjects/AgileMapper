namespace AgileObjects.AgileMapper.Api.Configuration
{
    using AgileMapper.Configuration.Api;

    public class ConfigStartingPoint
    {
        private readonly MapperContext _mapperContext;

        internal ConfigStartingPoint(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public InstanceCreationRuleSpecifier CreatingInstances => new InstanceCreationRuleSpecifier(_mapperContext);

        public MappingConfigStartingPoint Mapping => new MappingConfigStartingPoint(_mapperContext);
    }
}