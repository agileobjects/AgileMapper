namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    public class CompanyDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public AddressDto HeadOffice { get; set; }

        public EmployeeDto Ceo { get; set; }
    }
}