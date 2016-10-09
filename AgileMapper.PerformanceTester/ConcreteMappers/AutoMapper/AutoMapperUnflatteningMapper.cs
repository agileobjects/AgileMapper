using AmMapper = AutoMapper.Mapper;

namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AutoMapperUnflatteningMapper : UnflatteningMapperBase
    {
        public override void Initialise()
        {
            AmMapper.Initialize(cfg =>
            {
                cfg.CreateMap<ModelDto, ModelSubSubObject>()
                    .ForMember(dest => dest.CoolProperty, c => c.MapFrom(src => src.SubSubSubCoolProperty));

                cfg.CreateMap<ModelDto, ModelSubObject>()
                    .ForMember(dest => dest.ProperName, c => c.MapFrom(src => src.SubProperName));

                cfg.CreateMap<ModelDto, ModelObject>()
                    .ForMember(dest => dest.Sub, c => c.MapFrom(src => src))
                    .ForMember(dest => dest.Sub2, c => c.MapFrom(src => src))
                    .ForMember(dest => dest.SubWithExtraName, c => c.MapFrom(src => src));
            });
        }

        protected override ModelObject Unflatten(ModelDto dto)
        {
            return AmMapper.Map<ModelDto, ModelObject>(dto);
        }
    }
}