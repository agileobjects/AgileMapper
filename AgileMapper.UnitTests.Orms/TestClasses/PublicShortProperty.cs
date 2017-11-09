namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicShortProperty
    {
        [Key]
        public int Id { get; set; }


        public short Value { get; set; }
    }
}