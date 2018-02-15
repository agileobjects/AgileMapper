namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class PublicDouble
    {
        [Key]
        public int Id { get; set; }


        public double Value { get; set; }
    }
}