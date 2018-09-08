namespace AgileObjects.AgileMapper.PerformanceTester.Net45.ConcreteMappers.ExpressMapper
{
    using global::ExpressMapper;
    using PerformanceTesting.AbstractMappers;

    public class ExpressMapperInstantiation : MapperInstantiationBase
    {
        protected override object CreateMapperInstance() => new MappingServiceProvider();
    }
}