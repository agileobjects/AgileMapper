namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class Shape
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int? NumberOfSides { get; set; }

        public int? SideLength { get; set; }

        public int? Diameter { get; set; }
    }
}