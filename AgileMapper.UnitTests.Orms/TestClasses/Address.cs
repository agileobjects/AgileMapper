namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class Address
    {
        [Key]
        public int AddressId { get; set; }

        public string Line1 { get; set; }

        public string Line2 { get; set; }

        public string Postcode { get; set; }
    }
}