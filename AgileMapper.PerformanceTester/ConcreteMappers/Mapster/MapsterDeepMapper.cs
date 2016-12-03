namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Mapster
{
    using System.Collections.Generic;
    using AbstractMappers;
    using global::Mapster;
    using TestClasses;

    internal class MapsterDeepMapper : DeepMapperBase
    {
        public override void Initialise()
        {
            TypeAdapterConfig<Customer, CustomerDto>.NewConfig()
                .Map(dest => dest.Addresses, src => src.Addresses ?? new List<Address>())
                .Map(dest => dest.AddressesArray, src => src.AddressesArray ?? new Address[0])
                .Compile();
        }

        protected override CustomerDto Map(Customer customer)
        {
            return customer.Adapt<Customer, CustomerDto>();
        }
    }
}