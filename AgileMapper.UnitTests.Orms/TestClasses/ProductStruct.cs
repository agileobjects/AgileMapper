namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    public struct ProductStruct
    {
        public ProductStruct(string name)
        {
            ProductId = default(int);
            Name = name;
            Price = default(double);
        }

        public int ProductId { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }
    }
}