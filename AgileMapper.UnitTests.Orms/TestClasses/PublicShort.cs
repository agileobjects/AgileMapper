namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicShort
    {
        [Key]
        public int Id { get; set; }


        public short Value { get; set; }
    }
}