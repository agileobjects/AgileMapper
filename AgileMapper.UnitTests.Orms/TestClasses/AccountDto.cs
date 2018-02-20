namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.Collections.Generic;

    public class AccountDto
    {
        public int Id { get; set; }

        public PersonDto User { get; set; }

        public int DeliveryAddressCount { get; set; }

        public IEnumerable<AddressDto> DeliveryAddresses { get; set; }
    }
}