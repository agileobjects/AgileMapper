namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicDecimal
    {
        [Key]
        public int Id { get; set; }


        public decimal Value { get; set; }
    }
}