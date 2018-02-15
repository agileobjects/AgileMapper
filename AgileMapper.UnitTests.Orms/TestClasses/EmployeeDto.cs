namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System;

    public class EmployeeDto
    {
        public int Id { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Name { get; set; }

        public CompanyDto Company { get; set; }

        public AddressDto Address { get; set; }
    }
}