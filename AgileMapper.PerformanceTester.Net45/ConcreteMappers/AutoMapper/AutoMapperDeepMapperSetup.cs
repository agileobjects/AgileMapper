namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;
    using static TestClasses.Deep;

    internal class AutoMapperDeepMapperSetup : DeepMapperSetupBase
    {
        public override void Initialise()
        {
        }

        protected override void SetupDeepMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Address, Address>();
                cfg.CreateMap<Address, AddressDto>();
                cfg.CreateMap<Customer, CustomerDto>();
            });

            Mapper.Map<Customer, CustomerDto>(new Customer());
        }

        protected override void Reset() => Mapper.Reset();
    }
}