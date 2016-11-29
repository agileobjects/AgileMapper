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
                .Map(dest => dest.Addresses, src => new List<AddressDto>(), src => src.Addresses == null)
                .Map(dest => dest.Addresses, src => src.Addresses.Adapt<List<AddressDto>>())
                .Map(dest => dest.AddressesArray, src => new AddressDto[0], src => src.AddressesArray == null)
                .Map(dest => dest.AddressesArray, src => src.AddressesArray.Adapt<AddressDto[]>());
        }

        protected override CustomerDto Map(Customer customer)
        {
            return customer.Adapt<CustomerDto>();
        }
    }
}