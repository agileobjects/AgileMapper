namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicByte
    {
        [Key]
        public int Id { get; set; }

        public byte Value { get; set; }
    }
}