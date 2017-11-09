namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicIntProperty
    {
        [Key]
        public int Id { get; set; }


        public int Value { get; set; }
    }
}