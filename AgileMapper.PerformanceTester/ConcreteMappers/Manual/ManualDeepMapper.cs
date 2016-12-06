namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Manual
{
    using System.Collections.Generic;
    using System.Linq;
    using AbstractMappers;
    using TestClasses;

    internal class ManualDeepMapper : DeepMapperBase
    {
        public override void Initialise()
        {
        }

        protected override CustomerDto Map(Customer customer)
        {
            if (customer == null)
            {
                return null;
            }

            var dto = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name
            };

            if (customer.Address != null)
            {
                dto.AddressCity = customer.Address.City;
                dto.Address = new Address
                {
                    Id = customer.Address.Id,
                    Street = customer.Address.Street,
                    City = customer.Address.City,
                    Country = customer.Address.Country
                };
            }

            if (customer.HomeAddress != null)
            {
                dto.HomeAddress = new AddressDto
                {
                    Id = customer.HomeAddress.Id,
                    City = customer.HomeAddress.City,
                    Country = customer.HomeAddress.Country
                };
            }

            dto.Addresses = customer.Addresses != null
                ? customer
                    .Addresses
                    .Select(a => new AddressDto
                    {
                        Id = a.Id,
                        City = a.City,
                        Country = a.Country
                    })
                    .ToList()
                : new List<AddressDto>();

            dto.AddressesArray = (customer.AddressesArray != null)
                ? customer
                    .AddressesArray
                    .Select(a => new AddressDto
                    {
                        Id = a.Id,
                        City = a.City,
                        Country = a.Country
                    })
                    .ToArray()
                : Enumerable<AddressDto>.EmptyArray;

            return dto;
        }
    }
}