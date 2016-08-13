namespace AgileObjects.AgileMapper.PerformanceTester
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting...");

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

            const int NUMBER_OF_SETUPS = 1000;
            const int NUMBER_OF_MAPPINGS = 1000000;

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            for (var i = 0; i < NUMBER_OF_SETUPS; i++)
            {
                using (SetupMapper(source))
                {
                }
            }

            stopwatch.Stop();

            Console.WriteLine(
                $"Total setup time: {stopwatch.Elapsed.TotalSeconds}s " +
                $"({NUMBER_OF_SETUPS} setups, " +
                $"{(stopwatch.Elapsed.TotalSeconds / NUMBER_OF_SETUPS * 1000)}ms average)");

            using (var mapper = SetupMapper(source))
            {
                stopwatch.Reset();
                stopwatch.Start();

                for (var i = 0; i < NUMBER_OF_MAPPINGS; i++)
                {
                    mapper.Map(source).ToANew<CustomerViewModel>();
                }

                stopwatch.Stop();
            }

            Console.WriteLine(
                $"Total map time: {stopwatch.Elapsed.TotalSeconds}s " +
                $"({NUMBER_OF_MAPPINGS} mappings, " +
                $"{(stopwatch.Elapsed.TotalSeconds / NUMBER_OF_MAPPINGS * 1000)}ms average)");

            Console.WriteLine("Finished!");

            if (args.Length > 0)
            {
                Console.ReadLine();
            }
        }

        private static IMapper SetupMapper(Customer source)
        {
            var mapper = Mapper.CreateNew();

            mapper.WhenMapping
                .From<Address>()
                .ToANew<AddressViewModel>()
                .Map((a, avm) => a.Postcode.Value)
                .To(avm => avm.Postcode);

            mapper.Map(source).ToANew<CustomerViewModel>();

            return mapper;
        }
    }

    public class Customer
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<Address> Addresses { get; set; }
    }

    public sealed class Address
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
