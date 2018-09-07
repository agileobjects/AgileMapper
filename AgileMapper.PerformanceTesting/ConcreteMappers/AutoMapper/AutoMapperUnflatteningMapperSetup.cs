namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;
    using static TestClasses.Flattening;

    public class AutoMapperUnflatteningMapperSetup : UnflatteningMapperSetupBase
    {
        public override void Initialise()
        {
        }

        protected override ModelObject SetupUnflatteningMapper(ModelDto dto)
        {
            Mapper.Initialize(cfg => cfg.CreateMap<ModelObject, ModelDto>().ReverseMap());

            return Mapper.Map<ModelDto, ModelObject>(dto);
        }

        protected override void Reset() => Mapper.Reset();
    }
}