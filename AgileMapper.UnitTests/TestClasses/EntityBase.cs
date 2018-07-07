namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
#if NET35
    using System;

    internal sealed class KeyAttribute : Attribute { }
#else
    using System.ComponentModel.DataAnnotations;
#endif

    internal abstract class EntityBase
    {
        [Key]
        public int Id { get; set; }
    }
}

