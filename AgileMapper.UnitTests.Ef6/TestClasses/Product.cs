namespace AgileObjects.AgileMapper.UnitTests.Ef6.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        public string Name { get; set; }
    }
}