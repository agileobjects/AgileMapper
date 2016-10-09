namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ValueInjecter
{
    using System.Linq;
    using AbstractMappers;
    using Omu.ValueInjecter;
    using Omu.ValueInjecter.Injections;
    using TestClasses;

    internal class ValueInjecterDeepMapper : DeepMapperBase
    {
        public override void Initialise()
        {
        }

        protected override CustomerDto Map(Customer customer)
        {
            var dto = (CustomerDto)new CustomerDto().InjectFrom<FlatLoopInjection>(customer);
            dto.HomeAddress = (AddressDto)new AddressDto().InjectFrom(customer.HomeAddress);
            dto.Addresses = customer.Addresses.Select(a => (AddressDto)new AddressDto().InjectFrom(a)).ToList();
            dto.AddressesArray = customer.AddressesArray.Select(a => (AddressDto)new AddressDto().InjectFrom(a)).ToArray();

            return dto;
        }
    }
}