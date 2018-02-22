namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ExpressMapper
{
    using System.Collections.Generic;
    using AbstractMappers;
    using global::ExpressMapper;
    using global::ExpressMapper.Extensions;
    using TestClasses;

    internal class ExpressMapperDeepMapperSetup : DeepMapperSetupBase
    {
        public override void Initialise()
        {
        }

        protected override void Reset()
        {
            Mapper.Reset();
        }

        protected override void SetupDeepMapper()
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

            Mapper.Map<Customer, CustomerDto>(new Customer());
        }
    }
}