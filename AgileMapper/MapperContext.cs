namespace AgileObjects.AgileMapper
{
    using Caching;
    using Configuration;
    using DataSources;
    using Flattening;
    using Members;
    using ObjectPopulation;
    using TypeConversion;

    internal class MapperContext
    {
        public static readonly MapperContext Default = new MapperContext();

        public MapperContext()
        {
            Cache = new CacheSet();
            DataSources = new DataSourceFinder();
            NamingSettings = new NamingSettings();
            ObjectMapperFactory = new ObjectMapperFactory();
            ObjectFlattener = new ObjectFlattener();
            UserConfigurations = new UserConfigurationSet();
            ValueConverters = new ConverterSet();
            RuleSets = new MappingRuleSetCollection();
        }

        public CacheSet Cache { get; }

        public DataSourceFinder DataSources { get; }

        public NamingSettings NamingSettings { get; }

        public ObjectMapperFactory ObjectMapperFactory { get; }

        public ObjectFlattener ObjectFlattener { get; }

        public UserConfigurationSet UserConfigurations { get; }

        public ConverterSet ValueConverters { get; }

        public MappingRuleSetCollection RuleSets { get; }

        public void Reset()
        {
            Cache.Empty();
            DataSources.Reset();
            UserConfigurations.Reset();
            ObjectMapperFactory.Reset();
        }
    }
}