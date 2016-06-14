namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class OrderItem
    {
        public int OrderItemId { get; set; }

        public Order Order { get; set; }

        public string ProductId { get; set; }
    }
}