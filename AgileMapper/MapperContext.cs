namespace AgileObjects.AgileMapper
{
    using Api.Configuration;
    using DataSources;
    using ObjectPopulation;
    using TypeConversion;

    internal class MapperContext
    {
        public static readonly MapperContext Default = new MapperContext();

        public MapperContext()
        {
            DataSources = new DataSourceFinder();
            ComplexTypeFactory = new ComplexTypeFactory();
            ObjectMapperFactory = new ObjectMapperFactory();
            UserConfigurations = new UserConfigurationSet();
            ValueConverters = new ConverterSet();
            RuleSets = new MappingRuleSetCollection();
        }

        public GlobalContext GlobalContext => GlobalContext.Default;

        public DataSourceFinder DataSources { get; }

        public ComplexTypeFactory ComplexTypeFactory { get; }

        public ObjectMapperFactory ObjectMapperFactory { get; }

        public UserConfigurationSet UserConfigurations { get; }

        public ConverterSet ValueConverters { get; }

        public MappingRuleSetCollection RuleSets { get; }
    }
}