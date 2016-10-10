namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Mapster
{
    using AbstractMappers;
    using global::Mapster;
    using TestClasses;

    internal class MapsterCtorMapper : CtorMapperBase
    {
        public override void Initialise()
        {
            TypeAdapterConfig<ValueObject, ConstructedObject>.NewConfig()
                .ConstructUsing(src => new ConstructedObject(src.Value));
        }

        protected override ConstructedObject Construct(ValueObject valueObject)
        {
            return valueObject.Adapt<ConstructedObject>();
        }
    }
}