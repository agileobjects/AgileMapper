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
    }
}
