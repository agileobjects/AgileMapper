namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Deep;

    internal class AgileMapperDeepMapperSetup : DeepMapperSetupBase
    {
        private IMapper _mapper;

        public override void Initialise() => _mapper = Mapper.CreateNew();

        protected override void Reset() => _mapper.Dispose();

        protected override void SetupDeepMapper()
            => _mapper.GetPlanFor<Customer>().ToANew<CustomerDto>();
    }
}