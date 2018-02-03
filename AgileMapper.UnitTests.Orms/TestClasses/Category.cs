namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Category
    {
        private Category _parentCategory;

        public Category()
        {
            SubCategories = new List<Category>();
        }

        [Key]
        public int Id { get; set; }

        public int? ParentCategoryId { get; set; }

        public Category ParentCategory
        {
            get => _parentCategory;
            set
            {
                _parentCategory = value;

                if (value != null)
                {
                    ParentCategoryId = _parentCategory.Id;
                }
            }
        }

        public string Name { get; set; }

        public void AddSubCategories(params Category[] subCategories)
        {
            foreach (var subCategory in subCategories)
            {
                subCategory.ParentCategory = this;
                SubCategories.Add(subCategory);
            }
        }

        public ICollection<Category> SubCategories { get; set; }
    }
}