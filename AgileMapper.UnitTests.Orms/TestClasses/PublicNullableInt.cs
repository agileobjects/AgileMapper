namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicNullableInt
    {
        [Key]
        public int Id { get; set; }


        public int? Value { get; set; }
    }
}