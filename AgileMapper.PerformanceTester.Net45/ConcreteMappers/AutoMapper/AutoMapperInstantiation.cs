namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;

    internal class AutoMapperInstantiation : MapperInstantiationBase
    {
        protected override object CreateMapperInstance() => new Mapper(new MapperConfiguration(cfg => { }));
    }
}