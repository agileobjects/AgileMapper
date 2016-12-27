namespace AgileObjects.AgileMapper
{
    using Caching;
    using Configuration;
    using DataSources;
    using Flattening;
    using Members;
    using Members.Sources;
    using ObjectPopulation;
    using TypeConversion;

    internal class MapperContext
    {
        internal static readonly MapperContext Default = new MapperContext(NamingSettings.Default);

        public MapperContext(NamingSettings namingSettings = null)
        {
            Cache = new CacheSet();
            DataSources = new DataSourceFinder();
            NamingSettings = namingSettings ?? new NamingSettings();
            QualifiedMemberFactory = new QualifiedMemberFactory(this);
            RootMembersSource = new RootMembersSource(QualifiedMemberFactory);
            ObjectMapperFactory = new ObjectMapperFactory(this);
            ObjectFlattener = new ObjectFlattener();
            DerivedTypes = new DerivedTypesCache();
            UserConfigurations = new UserConfigurationSet(this);
            ValueConverters = new ConverterSet();
            RuleSets = new MappingRuleSetCollection();
        }

        public CacheSet Cache { get; }

        public DataSourceFinder DataSources { get; }

        public NamingSettings NamingSettings { get; }

        public QualifiedMemberFactory QualifiedMemberFactory { get; }

        public RootMembersSource RootMembersSource { get; }

        public ObjectMapperFactory ObjectMapperFactory { get; }

        public ObjectFlattener ObjectFlattener { get; }

        public DerivedTypesCache DerivedTypes { get; }

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