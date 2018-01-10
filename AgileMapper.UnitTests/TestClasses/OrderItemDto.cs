namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class OrderItemDto : DtoBase
    {
        public OrderEntity Order { get; set; }

        public ProductDto Product { get; set; }
    }
}