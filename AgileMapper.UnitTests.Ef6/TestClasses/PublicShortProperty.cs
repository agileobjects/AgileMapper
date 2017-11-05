namespace AgileObjects.AgileMapper.UnitTests.Ef6.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicShortProperty
    {
        [Key]
        public int Id { get; set; }


        public short Value { get; set; }
    }
}