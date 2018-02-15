namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Account
    {
        public Account()
        {
            DeliveryAddresses = new List<AccountAddress>();
        }

        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        public Person User { get; set; }

        public Account AddDeliveryAddress(Address address)
        {
            DeliveryAddresses.Add(new AccountAddress
            {
                Account = this,
                Address = address
            });

            return this;
        }

        public ICollection<AccountAddress> DeliveryAddresses { get; set; }
    }
}