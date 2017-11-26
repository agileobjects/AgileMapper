namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Category
    {
        public Category()
        {
            SubCategories = new List<Category>();
        }

        [Key]
        public int Id { get; set; }

        public int? ParentCategoryId { get; set; }

        public Category ParentCategory { get; set; }

        public string Name { get; set; }

        public ICollection<Category> SubCategories { get; set; }
    }
}