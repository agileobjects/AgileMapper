namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.Mapster
{
    using AbstractMappers;
    using global::Mapster;
    using static TestClasses.Ctor;

    public class MapsterCtorMapper : CtorMapperBase
    {
        public override void Initialise()
        {
            TypeAdapterConfig<ValueObject, ConstructedObject>.NewConfig()
                .Compile();
        }

        protected override ConstructedObject Construct(ValueObject valueObject)
            => valueObject.Adapt<ValueObject, ConstructedObject>();
    }
}