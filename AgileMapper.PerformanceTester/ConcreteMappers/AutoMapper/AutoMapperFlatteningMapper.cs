using AmMapper = AutoMapper.Mapper;

namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AutoMapperFlatteningMapper : FlatteningMapperBase
    {
        public override void Initialise()
        {
            AmMapper.Initialize(cfg => cfg.CreateMap<ModelObject, ModelDto>());
        }

        protected override ModelDto Flatten(ModelObject model)
        {
            return AmMapper.Map<ModelObject, ModelDto>(model);
        }
    }
}