namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Flattening;

    public class AgileMapperUnflatteningMapperSetup : UnflatteningMapperSetupBase
    {
        private IMapper _mapper;

        public override void Initialise() => _mapper = Mapper.CreateNew();

        protected override ModelObject SetupUnflatteningMapper(ModelDto dto)
        {
            _mapper.GetPlanFor<ModelDto>().ToANew<ModelObject>();

            return _mapper.Map(dto).ToANew<ModelObject>();
        }

        protected override void Reset() => _mapper.Dispose();
    }
}