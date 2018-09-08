namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Deep;

    public class AgileMapperDeepMapperSetup : DeepMapperSetupBase
    {
        private IMapper _mapper;

        public override void Initialise() => _mapper = Mapper.CreateNew();

        protected override CustomerDto SetupDeepMapper(Customer customer)
        {
            _mapper.GetPlanFor<Customer>().ToANew<CustomerDto>();

            return _mapper.Map(customer).ToANew<CustomerDto>();
        }

        protected override void Reset() => _mapper.Dispose();
    }
}