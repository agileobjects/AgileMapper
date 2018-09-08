namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.Mapster
{
    using AbstractMappers;
    using global::Mapster;

    public class MapsterMapperInstantiation : MapperInstantiationBase
    {
        protected override object CreateMapperInstance() => new Adapter(new TypeAdapterConfig());
    }
}