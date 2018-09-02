namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Mapster
{
    using AbstractMappers;
    using global::Mapster;

    internal class MapsterMapperInstantiation : MapperInstantiationBase
    {
        protected override object CreateMapperInstance() => new Adapter(new TypeAdapterConfig());
    }
}