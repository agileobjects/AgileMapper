namespace AgileObjects.AgileMapper
{
    using Api.Configuration;
    using Caching;
    using DataSources;
    using Flattening;
    using ObjectPopulation;
    using TypeConversion;

    internal class MapperContext
    {
        public static readonly MapperContext Default = new MapperContext();

        public MapperContext()
        {
            DataSources = new DataSourceFinder();
            Cache = GlobalContext.CreateCache();
            ObjectMapperFactory = new ObjectMapperFactory();
            ObjectFlattener = new ObjectFlattener(GlobalContext.MemberFinder);
            UserConfigurations = new UserConfigurationSet();
            ValueConverters = new ConverterSet();
            RuleSets = new MappingRuleSetCollection();
        }

        public GlobalContext GlobalContext => GlobalContext.Default;

        public ICache Cache { get; }

        public DataSourceFinder DataSources { get; }

        public ObjectMapperFactory ObjectMapperFactory { get; }

        public ObjectFlattener ObjectFlattener { get; }

        public UserConfigurationSet UserConfigurations { get; }

        public ConverterSet ValueConverters { get; }

        public MappingRuleSetCollection RuleSets { get; }

        public void Reset()
        {
            Cache.Empty();
            UserConfigurations.Reset();
        }
    }
}