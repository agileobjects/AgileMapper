namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AgileMapperDeepMapper : DeepMapperBase
    {
        public override void Initialise()
        {
        }

        protected override CustomerDto Map(Customer customer)
        {
            return Mapper.Map(customer).ToANew<CustomerDto>();
        }
    }
}