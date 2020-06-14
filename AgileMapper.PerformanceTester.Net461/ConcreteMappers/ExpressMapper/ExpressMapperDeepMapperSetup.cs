namespace AgileObjects.AgileMapper.PerformanceTester.Net45.ConcreteMappers.ExpressMapper
{
    using System.Collections.Generic;
    using global::ExpressMapper;
    using global::ExpressMapper.Extensions;
    using PerformanceTesting.AbstractMappers;
    using static PerformanceTesting.TestClasses.Deep;

    public class ExpressMapperDeepMapperSetup : DeepMapperSetupBase
    {
        public override void Initialise()
        {
        }

        protected override CustomerDto SetupDeepMapper(Customer customer)
        {
            Mapper
                .Register<Customer, CustomerDto>()
                .Member(dest => dest.AddressCity, src => src.Address.City)
                .Member(
                    dest => dest.Addresses,
                    src => src.Addresses != null
                        ? src.Addresses.Map<ICollection<Address>, List<AddressDto>>()
                        : new List<AddressDto>())
                .Member(
                    dest => dest.AddressesArray,
                    src => src.AddressesArray != null
                        ? src.AddressesArray.Map<Address[], AddressDto[]>()
                        : new AddressDto[0]);

            Mapper.Compile();

            return Mapper.Map<Customer, CustomerDto>(customer);
        }

        protected override void Reset() => Mapper.Reset();
    }
}