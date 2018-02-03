namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.Collections.Generic;

    public class CategoryDto
    {
        public int Id { get; set; }

        public int? ParentCategoryId { get; set; }

        public CategoryDto ParentCategory { get; set; }

        public string Name { get; set; }

        public IEnumerable<CategoryDto> SubCategories { get; set; }
    }
}