namespace AgileObjects.AgileMapper
{
    using Api.Configuration;
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
            DataSources = new DataSourceFinder();
            NamingSettings = new NamingSettings();
            Cache = GlobalContext.CreateCache();
            ObjectMapperFactory = new ObjectMapperFactory();
            ObjectFlattener = new ObjectFlattener(this);
            UserConfigurations = new UserConfigurationSet();
            ValueConverters = new ConverterSet();
            RuleSets = new MappingRuleSetCollection();
        }

        public GlobalContext GlobalContext => GlobalContext.Default;

        public ICache Cache { get; }

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
            UserConfigurations.Reset();
        }
    }
}