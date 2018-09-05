namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ExpressMapper
{
    using AbstractMappers;
    using global::ExpressMapper;

    internal class ExpressMapperInstantiation : MapperInstantiationBase
    {
        protected override object CreateMapperInstance() => new MappingServiceProvider();
    }
}