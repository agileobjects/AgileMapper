namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicBoolProperty
    {
        [Key]
        public int Id { get; set; }

        public bool Value { get; set; }
    }
}