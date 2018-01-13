namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System.Collections.Generic;

    internal class CategoryDto : DtoBase
    {
        public string Name { get; set; }

        public int? ParentId { get; set; }

        public ICollection<int> ChildCategoriesIds { get; set; }
    }
}