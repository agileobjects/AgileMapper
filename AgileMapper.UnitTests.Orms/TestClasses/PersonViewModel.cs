namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    public class PersonViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Name { get; set; }

        public void SetName(string name) => Name = name;

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