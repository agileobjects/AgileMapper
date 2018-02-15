namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class Person
    {
        [Key]
        public int PersonId { get; set; }

        public string GetTitle() => "Dr";

        public string Name { get; set; }

        public int? AddressId { get; set; }

        public Address Address { get; set; }
    }
}
