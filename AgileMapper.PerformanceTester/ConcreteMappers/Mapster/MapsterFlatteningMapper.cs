namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Mapster
{
    using AbstractMappers;
    using global::Mapster;
    using TestClasses;

    internal class MapsterFlatteningMapper : FlatteningMapperBase
    {
        public override void Initialise()
        {
            TypeAdapterConfig<ModelObject, ModelDto>.NewConfig()
                .Map(dest => dest.Sub2ProperName, src => src.Sub2.ProperName)
                .Map(dest => dest.SubWithExtraNameProperName, src => src.SubWithExtraName.ProperName);
        }

        protected override ModelDto Flatten(ModelObject model)
        {
            return model.Adapt<ModelDto>();
        }
    }
}