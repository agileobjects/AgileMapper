namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class OrderItemEntity : EntityBase
    {
        public OrderEntity Order { get; set; }

        public ProductEntity Product { get; set; }
    }
}