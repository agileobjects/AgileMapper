namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Mapster
{
    using AbstractMappers;
    using global::Mapster;
    using static TestClasses.Ctor;

    internal class MapsterCtorMapper : CtorMapperBase
    {
        public override void Initialise()
        {
            TypeAdapterConfig<ValueObject, ConstructedObject>.NewConfig()
                .Compile();
        }

        protected override ConstructedObject Construct(ValueObject valueObject)
        {
            return valueObject.Adapt<ValueObject, ConstructedObject>();
        }
    }
}