namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;
    using static TestClasses.Deep;

    public class AutoMapperDeepMapperSetup : DeepMapperSetupBase
    {
        public override void Initialise()
        {
        }

        protected override CustomerDto SetupDeepMapper(Customer customer)
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Address, Address>();
                cfg.CreateMap<Address, AddressDto>();
                cfg.CreateMap<Customer, CustomerDto>();
            });

            return Mapper.Map<Customer, CustomerDto>(customer);
        }

        protected override void Reset() => Mapper.Reset();
    }
}