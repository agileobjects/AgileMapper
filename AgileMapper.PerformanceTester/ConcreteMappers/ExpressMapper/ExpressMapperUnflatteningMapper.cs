using ExMapper = ExpressMapper.Mapper;

namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ExpressMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class ExpressMapperUnflatteningMapper : UnflatteningMapperBase
    {
        public override void Initialise()
        {
            ExMapper
                .Register<ModelDto, ModelObject>()
                .Member(dest => dest.Sub.ProperName, src => src.SubProperName)
                .Member(dest => dest.Sub2.ProperName, src => src.Sub2ProperName)
                .Member(dest => dest.SubWithExtraName.ProperName, src => src.SubWithExtraNameProperName)
                .Member(dest => dest.Sub.SubSub.CoolProperty, src => src.SubSubSubCoolProperty);

            ExMapper.Compile();
        }

        protected override ModelObject Unflatten(ModelDto dto)
        {
            return ExMapper.Map<ModelDto, ModelObject>(dto);
        }
    }
}