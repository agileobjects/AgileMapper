namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public string ProductName { get; set; }
    }
}