namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class OrderItemEntity : EntityBase
    {
        public int OrderId { get; set; }

        public OrderEntity Order { get; set; }

        public int ProductId { get; set; }

        public ProductEntity Product { get; set; }
    }
}