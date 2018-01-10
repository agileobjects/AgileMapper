namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    internal class SaveOrderItemRequest : DtoBase
    {
        public int OrderId { get; set; }

        public int ProductId { get; set; }
    }
}