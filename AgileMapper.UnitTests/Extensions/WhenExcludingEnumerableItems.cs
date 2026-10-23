namespace AgileObjects.AgileMapper.UnitTests.Extensions
{
    using System.Linq;
    using AgileMapper.Extensions;
    using Common;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenExcludingEnumerableItems
    {
        [Fact]
        public void ShouldPreserveNullItemsNotExcluded()
        {
            var items = new string[] { "first", null, "last" };
            var result = items.Exclude(new[] { "missing" });

            result.SequenceEqual(new string[] { "first", null, "last" }).ShouldBeTrue();
        }

        [Fact]
        public void ShouldExcludeOnlyTheRequestedNumberOfNullItems()
        {
            var items = new string[] { null, "first", null };
            var result = items.Exclude(new string[] { null });

            result.SequenceEqual(new string[] { "first", null }).ShouldBeTrue();
        }
    }
}
