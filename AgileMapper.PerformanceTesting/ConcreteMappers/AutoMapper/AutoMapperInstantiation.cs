namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;

    public class AutoMapperInstantiation : MapperInstantiationBase
    {
        protected override object CreateMapperInstance() => new Mapper(new MapperConfiguration(cfg => { }));
    }
}