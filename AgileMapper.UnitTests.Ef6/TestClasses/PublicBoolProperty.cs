namespace AgileObjects.AgileMapper.UnitTests.Ef6.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicBoolProperty
    {
        [Key]
        public int Id { get; set; }

        public bool Value { get; set; }
    }
}