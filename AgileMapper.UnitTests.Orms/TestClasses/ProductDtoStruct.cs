namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    public struct ProductDtoStruct
    {
        public ProductDtoStruct(int productId)
        {
            ProductId = productId;
            Name = default(string);
        }

        public int ProductId { get; set; }

        public string Name { get; set; }
    }
}