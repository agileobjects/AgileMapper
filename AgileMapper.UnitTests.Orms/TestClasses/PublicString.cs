namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicString
    {
        [Key]
        public int Id { get; set; }


        public string Value { get; set; }
    }
}