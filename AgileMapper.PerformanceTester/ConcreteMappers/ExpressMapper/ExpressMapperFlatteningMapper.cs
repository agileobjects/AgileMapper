namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ExpressMapper
{
    using AbstractMappers;
    using global::ExpressMapper;
    using TestClasses;

    internal class ExpressMapperFlatteningMapper : FlatteningMapperBase
    {
        public override void Initialise()
        {
            Mapper
                .Register<ModelObject, ModelDto>()
                .Member(dest => dest.SubProperName, src => src.Sub.ProperName)
                .Member(dest => dest.Sub2ProperName, src => src.Sub2.ProperName)
                .Member(dest => dest.SubWithExtraNameProperName, src => src.SubWithExtraName.ProperName)
                .Member(dest => dest.SubSubSubCoolProperty, src => src.Sub.SubSub.CoolProperty);

            Mapper.Compile();
        }

        protected override ModelDto Flatten(ModelObject model)
        {
            return Mapper.Map<ModelObject, ModelDto>(model);
        }
    }
}