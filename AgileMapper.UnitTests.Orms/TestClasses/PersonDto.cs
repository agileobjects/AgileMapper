namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    public class PersonDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool HasAddress { get; set; }

        public AddressDto Address { get; set; }
    }
}