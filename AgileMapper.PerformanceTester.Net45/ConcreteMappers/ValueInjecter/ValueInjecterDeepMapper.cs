namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ValueInjecter
{
    using System.Collections.Generic;
    using System.Linq;
    using AbstractMappers;
    using Omu.ValueInjecter;
    using Omu.ValueInjecter.Injections;
    using static TestClasses.Deep;

    internal class ValueInjecterDeepMapper : DeepMapperBase
    {
        public override void Initialise()
        {
        }

        protected override CustomerDto Map(Customer customer)
        {
            var dto = (CustomerDto)new CustomerDto().InjectFrom<FlatLoopInjection>(customer);
            dto.HomeAddress = (AddressDto)new AddressDto().InjectFrom(customer.HomeAddress);

            dto.Addresses =
                customer.Addresses?.Select(a => (AddressDto)new AddressDto().InjectFrom(a)).ToList()
                ?? new List<AddressDto>();

            dto.AddressesArray =
                customer.AddressesArray?.Select(a => (AddressDto)new AddressDto().InjectFrom(a)).ToArray()
                ?? new AddressDto[0];

            return dto;
        }
    }
}