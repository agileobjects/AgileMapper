namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicStringProperty
    {
        [Key]
        public int Id { get; set; }


        public string Value { get; set; }
    }
}