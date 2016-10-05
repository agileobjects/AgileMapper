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
        internal static readonly MapperContext WithDefaultNamingSettings = new MapperContext(NamingSettings.Default);

        public MapperContext(NamingSettings namingSettings = null)
        {
            Cache = new CacheSet();
            DataSources = new DataSourceFinder();
            NamingSettings = namingSettings ?? new NamingSettings();
            RootMemberFactory = new RootQualifiedMemberFactory(this);
            ObjectMapperFactory = new ObjectMapperFactory(this);
#if !NET_STANDARD
            ObjectFlattener = new ObjectFlattener();
#endif
            UserConfigurations = new UserConfigurationSet();
            ValueConverters = new ConverterSet();
            RuleSets = new MappingRuleSetCollection();
        }

        public CacheSet Cache { get; }

        public DataSourceFinder DataSources { get; }

        public NamingSettings NamingSettings { get; }

        public RootQualifiedMemberFactory RootMemberFactory { get; }

        public ObjectMapperFactory ObjectMapperFactory { get; }

#if !NET_STANDARD
        public ObjectFlattener ObjectFlattener { get; }
#endif
        public UserConfigurationSet UserConfigurations { get; }

        public ConverterSet ValueConverters { get; }

        public MappingRuleSetCollection RuleSets { get; }

        public void Reset()
        {
            Cache.Empty();
            UserConfigurations.Reset();
            ObjectMapperFactory.Reset();
        }
    }
}