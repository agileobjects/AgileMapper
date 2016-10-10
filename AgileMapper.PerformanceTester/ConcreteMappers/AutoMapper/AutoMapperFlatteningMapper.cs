namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;
    using TestClasses;

    internal class AutoMapperFlatteningMapper : FlatteningMapperBase
    {
        public override void Initialise()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<ModelObject, ModelDto>());
        }

        protected override ModelDto Flatten(ModelObject model)
        {
            return Mapper.Map<ModelObject, ModelDto>(model);
        }
    }
}