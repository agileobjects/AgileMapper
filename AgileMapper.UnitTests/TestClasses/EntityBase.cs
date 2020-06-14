namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    internal abstract class EntityBase
    {
        [Key]
        public int Id { get; set; }
    }
}

