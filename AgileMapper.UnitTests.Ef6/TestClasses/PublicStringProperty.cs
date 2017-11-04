namespace AgileObjects.AgileMapper.UnitTests.Ef6.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicStringProperty
    {
        [Key]
        public int Id { get; set; }


        public string Value { get; set; }
    }
}