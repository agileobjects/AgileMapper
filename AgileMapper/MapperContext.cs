namespace AgileObjects.AgileMapper
{
    using System;
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
        private IMappingContext _queryProjectionMappingContext;
        private bool _isDisposed;

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
        }

        public IMapperInternal Mapper { get; set; }

        public CacheSet Cache { get; }

        public NamingSettings Naming { get; }

        public QualifiedMemberFactory QualifiedMemberFactory { get; }

        public RootMembersSource RootMembersSource { get; }

        public ObjectMapperFactory ObjectMapperFactory { get; }

        public InlineMapperContextSet InlineContexts
            => _inlineContexts ??= new InlineMapperContextSet(this);

        public UserConfigurationSet UserConfigurations { get; }

        public ComplexTypeConstructionFactory ConstructionFactory { get; }

        public ConverterSet ValueConverters { get; }

        public MappingRuleSetCollection RuleSets { get; }

        public IMappingContext QueryProjectionMappingContext
            => _queryProjectionMappingContext ??= new SimpleMappingContext(RuleSets.Project, this);

        public MapperContext Clone()
        {
            var context = new MapperContext();

            Naming.CloneTo(context.Naming);
            UserConfigurations.CloneTo(context.UserConfigurations);
            ValueConverters.CloneTo(context.ValueConverters);

            return context;
        }

        public void Dispose()
        {
            _isDisposed = true;
            Cache.Empty();
            UserConfigurations.Reset();
            ConstructionFactory.Reset();
            ObjectMapperFactory.Reset();
        }

        public void Reset() => _isDisposed = false;

        public MapperContext ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(Mapper));
            }

            return this;
        }
    }
}