namespace AgileObjects.AgileMapper
{
    using Caching;
    using Configuration;
    using Configuration.Inline;
    using DataSources;
    using Flattening;
    using Members;
    using Members.Sources;
    using ObjectPopulation;
    using TypeConversion;

    internal class MapperContext
    {
        internal static readonly MapperContext Default = new MapperContext(NamingSettings.Default);

        private ObjectFlattener _objectFlattener;
        private InlineMapperContextSet _inlineContexts;

        public MapperContext(NamingSettings namingSettings = null)
        {
            Cache = new CacheSet();
            DataSources = new DataSourceFinder(this);
            NamingSettings = namingSettings ?? new NamingSettings();
            QualifiedMemberFactory = new QualifiedMemberFactory(this);
            RootMembersSource = new RootMembersSource(QualifiedMemberFactory);
            ObjectMapperFactory = new ObjectMapperFactory(this);
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

        public ObjectFlattener ObjectFlattener => _objectFlattener ?? (_objectFlattener = new ObjectFlattener());

        public InlineMapperContextSet InlineContexts => _inlineContexts ?? (_inlineContexts = new InlineMapperContextSet(this));

        public UserConfigurationSet UserConfigurations { get; }

        public ConverterSet ValueConverters { get; }

        public MappingRuleSetCollection RuleSets { get; }

        public MapperContext Clone()
        {
            var context = new MapperContext();

            NamingSettings.CloneTo(context.NamingSettings);
            UserConfigurations.CloneTo(context.UserConfigurations);
            ValueConverters.CloneTo(context.ValueConverters);

            return context;
        }

        public void Reset()
        {
            Cache.Empty();
            UserConfigurations.Reset();
            ObjectMapperFactory.Reset();
        }
    }
}