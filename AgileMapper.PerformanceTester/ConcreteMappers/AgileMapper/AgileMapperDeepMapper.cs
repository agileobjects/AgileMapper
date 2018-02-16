namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AgileMapperDeepMapper : DeepMapperBase
    {
        private IMapper _mapper;

        public override void Initialise()
        {
            _mapper = Mapper.CreateNew();
        }

        protected override CustomerDto Map(Customer customer)
        {
            return _mapper.Map(customer).ToANew<CustomerDto>();
        }
    }
}