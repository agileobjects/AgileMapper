namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System.Collections.Generic;

    internal class CategoryEntity : EntityBase
    {
        public string Name { get; set; }

        public int? ParentId { get; set; }

        public CategoryEntity Parent { get; set; }

        public ICollection<CategoryEntity> ChildCategories { get; set; }
    }
}
