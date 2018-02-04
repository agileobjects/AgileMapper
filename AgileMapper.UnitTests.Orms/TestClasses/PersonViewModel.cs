namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    public class PersonViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? AddressId { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressPostcode
        {
            get;
            set;
        }
    }
}