namespace AgileObjects.AgileMapper.PerformanceTester.Net45.ConcreteMappers.ExpressMapper
{
    using global::ExpressMapper;
    using PerformanceTesting.AbstractMappers;
    using static PerformanceTesting.TestClasses.Flattening;

    public class ExpressMapperFlatteningMapper : FlatteningMapperBase
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
            => Mapper.Map<ModelObject, ModelDto>(model);
    }
}