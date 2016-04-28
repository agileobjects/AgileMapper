namespace AgileObjects.AgileMapper
{
    using DataSources;
    using ObjectPopulation;

    internal class MapperContext
    {
        public static readonly MapperContext Default = new MapperContext(GlobalContext.Default);

        private MapperContext(GlobalContext globalContext)
        {
            GlobalContext = globalContext;
            ObjectMapperFactory = new ObjectMapperFactory();
            ObjectFactory = new ObjectFactory();
            DataSources = new DataSourceFinder(globalContext.MemberFinder);
            RuleSets = new MappingRuleSetCollection();
        }

        public GlobalContext GlobalContext { get; }

        public ObjectMapperFactory ObjectMapperFactory { get; }

        public ObjectFactory ObjectFactory { get; }

        public DataSourceFinder DataSources { get; }

        public MappingRuleSetCollection RuleSets { get; }
    }
}