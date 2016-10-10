namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ExpressMapper
{
    using AbstractMappers;
    using global::ExpressMapper;
    using TestClasses;

    internal class ExpressMapperDeepMapper : DeepMapperBase
    {
        public override void Initialise()
        {
            Mapper
                .Register<Customer, CustomerDto>()
                .Member(dest => dest.AddressCity, src => src.Address.City);

            Mapper.Compile();
        }

        protected override CustomerDto Map(Customer customer)
        {
            return Mapper.Map<Customer, CustomerDto>(customer);
        }
    }
}