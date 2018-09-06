namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using UnitTests.Common;
    using static TestClasses.Deep;

    public abstract class DeepMapperBase : MapperTestBase
    {
        private readonly Customer _customer;

        protected DeepMapperBase()
        {
            _customer = new Customer
            {
                Address = new Address { City = "istanbul", Country = "turkey", Id = 1, Street = "istiklal cad." },
                HomeAddress = new Address { City = "urkdnsd", Country = "turkey", Id = 2, Street = "istiklal cad." },
                Id = 1,
                Name = "Eduardo Najera",
                Credit = 234.7m,
                Addresses = new List<Address>
                {
                    new Address { City = "idejhs", Country = "turkey", Id = 5, Street = "istiklal cad." },
                    new Address { City = "izmir", Country = "turkey", Id = 6, Street = "konak" }
                },
                AddressesArray = new[]
                {
                    new Address { City = "idjsdssy", Country = "turkey", Id = 3, Street = "istiklal cad." },
                    new Address { City = "izthrtgrerfedmir", Country = "turkey", Id = 4, Street = "konak" }
                }
            };
        }

        public override string Type => "deep";

        public override object Execute(Stopwatch timer) => Map(_customer);

        protected abstract CustomerDto Map(Customer customer);

        public override void Verify(object result)
        {
            var dto = (result as CustomerDto).ShouldNotBeNull();

            dto.Id.ShouldBe(1);
            dto.Name.ShouldBe("Eduardo Najera");
            dto.AddressCity.ShouldBe("istanbul");

            var address = dto.Address.ShouldNotBeNull();

            address.Id.ShouldBe(1);
            address.Street.ShouldBe("istiklal cad.");
            address.City.ShouldBe("istanbul");
            address.Country.ShouldBe("turkey");

            var homeAddress = dto.HomeAddress.ShouldNotBeNull();

            homeAddress.Id.ShouldBe(2);
            homeAddress.City.ShouldBe("urkdnsd");
            homeAddress.Country.ShouldBe("turkey");

            var addresses = dto.Addresses.ShouldNotBeEmpty();

            addresses.Count().ShouldBe(2);

            addresses.First().Id.ShouldBe(5);
            addresses.First().City.ShouldBe("idejhs");
            addresses.First().Country.ShouldBe("turkey");

            addresses.Second().Id.ShouldBe(6);
            addresses.Second().City.ShouldBe("izmir");
            addresses.Second().Country.ShouldBe("turkey");

            var addressesArr = dto.AddressesArray.ShouldNotBeEmpty();

            addressesArr.Count().ShouldBe(2);

            addressesArr.First().Id.ShouldBe(3);
            addressesArr.First().City.ShouldBe("idjsdssy");
            addressesArr.First().Country.ShouldBe("turkey");

            addressesArr.Second().Id.ShouldBe(4);
            addressesArr.Second().City.ShouldBe("izthrtgrerfedmir");
            addressesArr.Second().Country.ShouldBe("turkey");
        }
    }
}