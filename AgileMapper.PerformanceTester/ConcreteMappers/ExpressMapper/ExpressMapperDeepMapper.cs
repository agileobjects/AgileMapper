using ExMapper = ExpressMapper.Mapper;

namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ExpressMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class ExpressMapperDeepMapper : DeepMapperBase
    {
        public override void Initialise()
        {
            ExMapper
                .Register<Customer, CustomerDto>()
                .Member(dest => dest.AddressCity, src => src.Address.City);

            ExMapper.Compile();
        }

        protected override CustomerDto Map(Customer customer)
        {
            return ExMapper.Map<Customer, CustomerDto>(customer);
        }
    }
}