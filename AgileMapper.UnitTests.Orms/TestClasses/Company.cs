namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class Company
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int HeadOfficeId { get; set; }

        public Address HeadOffice { get; set; }

        public int? CeoId { get; set; }

        public Employee Ceo { get; set; }
    }
}
