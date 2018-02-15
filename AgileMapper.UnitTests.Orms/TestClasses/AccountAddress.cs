namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    public class AccountAddress
    {
        public int AccountId { get; set; }

        public Account Account { get; set; }

        public int AddressId { get; set; }

        public Address Address { get; set; }
    }
}