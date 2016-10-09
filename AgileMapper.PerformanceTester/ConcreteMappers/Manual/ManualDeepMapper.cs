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

            return new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                AddressCity = customer.Address?.City,
                Address = (customer.Address != null)
                    ? new Address
                    {
                        Id = customer.Address.Id,
                        Street = customer.Address.Street,
                        City = customer.Address.City,
                        Country = customer.Address.Country
                    } : null,
                HomeAddress = (customer.HomeAddress != null)
                ? new AddressDto
                {
                    Id = customer.HomeAddress.Id,
                    City = customer.HomeAddress.City,
                    Country = customer.HomeAddress.Country
                } : null,
                Addresses = customer
                    .Addresses?
                    .Select(a => new AddressDto
                    {
                        Id = a.Id,
                        City = a.City,
                        Country = a.Country
                    })
                    .ToList() ?? new List<AddressDto>(),
                AddressesArray = customer
                    .AddressesArray?
                    .Select(a => new AddressDto
                    {
                        Id = a.Id,
                        City = a.City,
                        Country = a.Country
                    })
                    .ToArray() ?? new AddressDto[0]
            };
        }
    }
}