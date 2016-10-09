using ExMapper = ExpressMapper.Mapper;

namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ExpressMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class ExpressMapperFlatteningMapper : FlatteningMapperBase
    {
        public override void Initialise()
        {
            ExMapper
                .Register<ModelObject, ModelDto>()
                .Member(dest => dest.SubProperName, src => src.Sub.ProperName)
                .Member(dest => dest.Sub2ProperName, src => src.Sub2.ProperName)
                .Member(dest => dest.SubWithExtraNameProperName, src => src.SubWithExtraName.ProperName)
                .Member(dest => dest.SubSubSubCoolProperty, src => src.Sub.SubSub.CoolProperty);

            ExMapper.Compile();
        }

        protected override ModelDto Flatten(ModelObject model)
        {
            return ExMapper.Map<ModelObject, ModelDto>(model);
        }
    }
}