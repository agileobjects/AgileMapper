namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    public struct ProductDtoStruct
    {
        public ProductDtoStruct(string name)
        {
            ProductId = default(int);
            Name = name;
        }

        public int ProductId { get; set; }

        public string Name { get; set; }
    }
}