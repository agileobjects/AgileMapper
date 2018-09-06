namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Deep;

    public class AgileMapperDeepMapper : DeepMapperBase
    {
        private IMapper _mapper;

        public override void Initialise() => _mapper = Mapper.CreateNew();

        protected override CustomerDto Map(Customer customer)
            => _mapper.Map(customer).ToANew<CustomerDto>();
    }
}