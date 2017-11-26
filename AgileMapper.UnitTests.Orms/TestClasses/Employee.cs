namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Employee
    {
        [Key]
        public int Id { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Name { get; set; }

        public int CompanyId { get; set; }

        [Required]
        public Company Company { get; set; }

        public int AddressId { get; set; }

        public Address Address { get; set; }
    }
}