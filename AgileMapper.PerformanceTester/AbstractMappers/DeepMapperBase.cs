namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using static TestClasses.Deep;

    internal abstract class DeepMapperBase : MapperTestBase
    {
        private readonly Customer _customer;

        protected DeepMapperBase()
        {
            _customer = new Customer
            {
                Address = new Address { City = "istanbul", Country = "turkey", Id = 1, Street = "istiklal cad." },
                HomeAddress = new Address { City = "istanbul", Country = "turkey", Id = 2, Street = "istiklal cad." },
                Id = 1,
                Name = "Eduardo Najera",
                Credit = 234.7m,
                Addresses = new List<Address>
                {
                    new Address { City = "istanbul", Country = "turkey", Id = 5, Street = "istiklal cad." },
                    new Address { City = "izmir", Country = "turkey", Id = 6, Street = "konak" }
                },
                AddressesArray = new[]
                {
                    new Address { City = "istanbul", Country = "turkey", Id = 3, Street = "istiklal cad." },
                    new Address { City = "izmir", Country = "turkey", Id = 4, Street = "konak" }
                }
            };
        }

        public override object Execute(Stopwatch timer) => Map(_customer);

        protected abstract CustomerDto Map(Customer customer);

        public override void Verify(object result)
        {
            throw new NotImplementedException();
        }
    }
}