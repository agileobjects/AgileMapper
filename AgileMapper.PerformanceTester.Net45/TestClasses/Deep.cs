namespace AgileObjects.AgileMapper.PerformanceTester.TestClasses
{
    using System.Collections.Generic;

    public static class Deep
    {
        public class Customer
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public decimal? Credit { get; set; }

            public Address Address { get; set; }

            public Address HomeAddress { get; set; }

            public ICollection<Address> Addresses { get; set; }

            public Address[] AddressesArray { get; set; }
        }

        public class Address
        {
            public int Id { get; set; }

            public string Street { get; set; }

            public string City { get; set; }

            public string Country { get; set; }
        }

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

        public class AddressDto
        {
            public int Id { get; set; }

            public string City { get; set; }

            public string Country { get; set; }
        }
    }
}
