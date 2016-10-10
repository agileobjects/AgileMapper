namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;
    using TestClasses;

    internal class AutoMapperDeepMapper : DeepMapperBase
    {
        public override void Initialise()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Address, Address>();
                cfg.CreateMap<Address, AddressDto>();
                cfg.CreateMap<Customer, CustomerDto>();
            });
        }

        protected override CustomerDto Map(Customer customer)
        {
            return Mapper.Map<Customer, CustomerDto>(customer);
        }
    }
}