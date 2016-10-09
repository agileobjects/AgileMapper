namespace AgileObjects.AgileMapper.PerformanceTester.TestClasses
{
    using System.Collections.Generic;

    public class CustomerDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Address Address { get; set; }

        public string AddressCity { get; set; }

        public AddressDto HomeAddress { get; set; }

        public List<AddressDto> Addresses { get; set; }

        public AddressDto[] AddressesArray { get; set; }
    }
}