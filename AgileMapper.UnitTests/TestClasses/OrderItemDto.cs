namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using Common.TestClasses;

    internal class OrderItemDto : DtoBase
    {
        public int OrderId { get; set; }

        public OrderDto Order { get; set; }

        public ProductDto Product { get; set; }
    }
}