namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using static TestClasses.Deep;

    internal class AgileMapperDeepMapper : DeepMapperBase
    {
        private IMapper _mapper;

        public override void Initialise() => _mapper = Mapper.CreateNew();

        protected override CustomerDto Map(Customer customer) 
            => _mapper.Map(customer).ToANew<CustomerDto>();
    }
}