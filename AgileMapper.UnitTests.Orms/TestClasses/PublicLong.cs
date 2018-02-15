namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicLong
    {
        [Key]
        public int Id { get; set; }


        public long Value { get; set; }
    }
}