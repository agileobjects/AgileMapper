using AmMapper = AutoMapper.Mapper;

namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AutoMapperDeepMapper : DeepMapperBase
    {
        public override void Initialise()
        {
            AmMapper.Initialize(cfg =>
            {
                cfg.CreateMap<Address, Address>();
                cfg.CreateMap<Address, AddressDto>();
                cfg.CreateMap<Customer, CustomerDto>();
            });
        }

        protected override CustomerDto Map(Customer customer)
        {
            return AmMapper.Map<Customer, CustomerDto>(customer);
        }
    }
}