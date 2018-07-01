namespace AgileObjects.AgileMapper
{
    using Caching;
    using Configuration;
    using Configuration.Inline;
    using Members;
    using Members.Sources;
    using ObjectPopulation;
    using ObjectPopulation.ComplexTypes;
    using TypeConversion;

    internal class MapperContext
    {
        private InlineMapperContextSet _inlineContexts;

        public MapperContext()
        {
            Cache = new CacheSet();
            Naming = new NamingSettings(Cache);
            QualifiedMemberFactory = new QualifiedMemberFactory(this);
            RootMembersSource = new RootMembersSource(QualifiedMemberFactory);
            ObjectMapperFactory = new ObjectMapperFactory(Cache);
            UserConfigurations = new UserConfigurationSet(this);
            ConstructionFactory = new ComplexTypeConstructionFactory(Cache);
            ValueConverters = new ConverterSet(UserConfigurations);
            RuleSets = MappingRuleSetCollection.Default;
            QueryProjectionMappingContext = new SimpleMappingContext(RuleSets.Project, this);
        }

        public IMapper Mapper { get; set; }

        public CacheSet Cache { get; }

        public NamingSettings Naming { get; }

        public QualifiedMemberFactory QualifiedMemberFactory { get; }

        public RootMembersSource RootMembersSource { get; }

        public ObjectMapperFactory ObjectMapperFactory { get; }

        public InlineMapperContextSet InlineContexts => _inlineContexts ?? (_inlineContexts = new InlineMapperContextSet(this));

        public UserConfigurationSet UserConfigurations { get; }

        public ComplexTypeConstructionFactory ConstructionFactory { get; }

        public ConverterSet ValueConverters { get; }

        public MappingRuleSetCollection RuleSets { get; }

        public IMappingContext QueryProjectionMappingContext { get; }

        public MapperContext Clone()
        {
            var context = new MapperContext();

            Naming.CloneTo(context.Naming);
            UserConfigurations.CloneTo(context.UserConfigurations);
            ValueConverters.CloneTo(context.ValueConverters);

            return context;
        }

        public void Reset()
        {
            Cache.Empty();
            UserConfigurations.Reset();
            ConstructionFactory.Reset();
            ObjectMapperFactory.Reset();
        }
    }
}