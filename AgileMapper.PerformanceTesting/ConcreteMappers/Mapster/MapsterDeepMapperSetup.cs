namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.Mapster
{
    using System.Collections.Generic;
    using AbstractMappers;
    using global::Mapster;
    using static TestClasses.Deep;

    public class MapsterDeepMapperSetup : DeepMapperSetupBase
    {
        public override void Initialise()
        {
        }

        protected override CustomerDto SetupDeepMapper(Customer customer)
        {
            TypeAdapterConfig<Customer, CustomerDto>.NewConfig()
                .Map(dest => dest.Addresses, src => src.Addresses ?? new List<Address>())
                .Map(dest => dest.AddressesArray, src => src.AddressesArray ?? new Address[0])
                .Compile();

            return customer.Adapt<Customer, CustomerDto>();
        }

        protected override void Reset()
            => TypeAdapterConfig<Customer, CustomerDto>.Clear();
    }
}