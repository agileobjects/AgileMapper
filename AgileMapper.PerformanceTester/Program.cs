namespace AgileObjects.AgileMapper.PerformanceTester
{
    using System;
    using System.Collections.Generic;

    public class Program
    {
        public static void Main(string[] args)
        {
            var source = new Customer
            {
                Id = Guid.NewGuid(),
                Name = "Goldblum",
                Addresses = new List<Address>
                {
                    new Address { Line1  = "Over here", Line2 = "Over there", Postcode = new Postcode("HH3 4RH") },
                    new Address { Line1  = "Somewhere", Line2 = "Nowhere", Postcode = new Postcode("SW4 6NW") },
                }
            };

            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Address>()
                    .ToANew<AddressViewModel>()
                    .Map((a, avm) => a.Postcode.Value)
                    .To(avm => avm.Postcode);

                mapper.Map(source).ToANew<CustomerViewModel>();

                for (var i = 0; i < 10000; i++)
                {
                    mapper.Map(source).ToANew<CustomerViewModel>();
                }

                Console.WriteLine("Finished!");
            }
        }
    }

    public class Customer
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<Address> Addresses { get; set; }
    }

    public class Address
    {
        public string Line1 { get; set; }

        public string Line2 { get; set; }

        public Postcode Postcode { get; set; }
    }

    public class Postcode
    {
        public Postcode(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    public class CustomerViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<AddressViewModel> Addresses { get; set; }
    }

    public class AddressViewModel
    {
        public string Line1 { get; set; }

        public string Line2 { get; set; }

        public string Postcode { get; set; }
    }
}
