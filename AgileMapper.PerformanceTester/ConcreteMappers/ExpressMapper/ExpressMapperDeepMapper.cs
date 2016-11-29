namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ExpressMapper
{
    using System.Collections.Generic;
    using AbstractMappers;
    using global::ExpressMapper;
    using TestClasses;

    internal class ExpressMapperDeepMapper : DeepMapperBase
    {
        public override void Initialise()
        {
            Mapper
                .Register<Customer, CustomerDto>()
                .Member(dest => dest.AddressCity, src => src.Address.City)
                .Member(
                    dest => dest.Addresses,
                    src => src.Addresses != null
                        ? Mapper.Map<ICollection<Address>, List<AddressDto>>(src.Addresses)
                        : new List<AddressDto>())
                .Member(
                    dest => dest.AddressesArray,
                    src => src.AddressesArray != null
                        ? Mapper.Map<Address[], AddressDto[]>(src.AddressesArray)
                        : new AddressDto[0]);

            Mapper.Compile();
        }

        protected override CustomerDto Map(Customer customer)
        {
            return Mapper.Map<Customer, CustomerDto>(customer);
        }
    }
}