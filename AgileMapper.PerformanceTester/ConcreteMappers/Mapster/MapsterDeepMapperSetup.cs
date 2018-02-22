namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Mapster
{
    using System.Collections.Generic;
    using AbstractMappers;
    using global::Mapster;
    using TestClasses;

    internal class MapsterDeepMapperSetup : DeepMapperSetupBase
    {
        public override void Initialise()
        {
        }

        protected override void Reset()
        {
            TypeAdapterConfig<Customer, CustomerDto>.Clear();
        }

        protected override void SetupDeepMapper()
        {
            TypeAdapterConfig<Customer, CustomerDto>.NewConfig()
                .Map(dest => dest.Addresses, src => src.Addresses ?? new List<Address>())
                .Map(dest => dest.AddressesArray, src => src.AddressesArray ?? new Address[0])
                .Compile();

            new Customer().Adapt<Customer, CustomerDto>();
        }
    }
}