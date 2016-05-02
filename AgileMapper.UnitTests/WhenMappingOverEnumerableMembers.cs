namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingOverEnumerableMembers
    {
        [Fact]
        public void ShouldOverwriteAndConvertACollection()
        {
            var source = new PublicProperty<IEnumerable<string>>
            {
                Value = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() }
            };

            var target = new PublicField<ICollection<Guid>>
            {
                Value = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            };

            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeSameAs(target.Value);
            result.Value.SequenceEqual(r => r.ToString(), source.Value).ShouldBeTrue();
        }

        [Fact]
        public void ShouldHandleANullSourceMember()
        {
            var source = new PublicField<IEnumerable<int>> { Value = null };

            var target = new PublicProperty<ICollection<int>>
            {
                Value = new[] { 1, 2, 4, 8 }
            };

            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleNoMatchingSourceMember()
        {
            var source = new { DooDeeDoo = "Jah jah jah" };

            var target = new PublicProperty<List<long>>
            {
                Value = new List<long> { 1, 2, 3 }
            };

            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeSameAs(target.Value);
        }
    }
}
