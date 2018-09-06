namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Deep;

    public class AgileMapperDeepMapperSetup : DeepMapperSetupBase
    {
        private IMapper _mapper;

        public override void Initialise() => _mapper = Mapper.CreateNew();

        protected override void SetupDeepMapper()
            => _mapper.GetPlanFor<Customer>().ToANew<CustomerDto>();

        protected override void Reset() => _mapper.Dispose();
    }
}