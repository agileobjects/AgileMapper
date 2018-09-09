namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.Mapster
{
    using AbstractMappers;
    using global::Mapster;
    using static TestClasses.Flattening;

    public class MapsterFlatteningMapper : FlatteningMapperBase
    {
        public override void Initialise()
        {
            TypeAdapterConfig<ModelObject, ModelDto>.NewConfig()
                .Compile();
        }

        protected override ModelDto Flatten(ModelObject model)
            => model.Adapt<ModelObject, ModelDto>();
    }
}