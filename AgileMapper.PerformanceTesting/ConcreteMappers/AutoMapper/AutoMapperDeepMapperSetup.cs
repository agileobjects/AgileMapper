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