namespace AgileObjects.AgileMapper
{
    using Api.Configuration;
    using Caching;
    using DataSources;
    using ObjectPopulation;
    using TypeConversion;

    internal class MapperContext
    {
        public static readonly MapperContext Default = new MapperContext();

        public MapperContext()
        {
            DataSources = new DataSourceFinder();
            Cache = GlobalContext.CreateCache();
            ComplexTypeFactory = new ComplexTypeFactory();
            ObjectMapperFactory = new ObjectMapperFactory();
            UserConfigurations = new UserConfigurationSet(GlobalContext.MemberFinder);
            ValueConverters = new ConverterSet();
            RuleSets = new MappingRuleSetCollection();
        }

        public GlobalContext GlobalContext => GlobalContext.Default;

        public ICache Cache { get; }

        public DataSourceFinder DataSources { get; }

        public ComplexTypeFactory ComplexTypeFactory { get; }

        public ObjectMapperFactory ObjectMapperFactory { get; }

        public UserConfigurationSet UserConfigurations { get; }

        public ConverterSet ValueConverters { get; }

        public MappingRuleSetCollection RuleSets { get; }
    }
}