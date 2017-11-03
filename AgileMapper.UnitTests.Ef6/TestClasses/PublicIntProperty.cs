namespace AgileObjects.AgileMapper.UnitTests.Ef6.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicIntProperty
    {
        [Key]
        public int Id { get; set; }


        public int Value { get; set; }
    }
}