namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AgileMapperUnflatteningMapperSetup : UnflatteningMapperSetupBase
    {
        private IMapper _mapper;

        public override void Initialise()
        {
            _mapper = Mapper.CreateNew();
        }

        protected override void Reset()
        {
            _mapper.Dispose();
        }

        protected override void SetupUnflatteningMapper()
        {
            _mapper.GetPlanFor<ModelDto>().ToANew<ModelObject>();

            _mapper.Map(new ModelDto()).ToANew<ModelObject>();
        }
    }
}