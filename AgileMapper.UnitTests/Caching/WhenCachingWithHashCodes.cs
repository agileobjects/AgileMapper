namespace AgileObjects.AgileMapper.UnitTests.Caching
{
    using AgileMapper.Caching;
    using Common;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenCachingWithHashCodes
    {
        [Fact]
        public void ShouldRetrieveFromAnEvenNumberedLengthCache()
        {
            ICache<TestKey, int> cache = new HashCodeArrayCache<TestKey, int>();

            cache.GetOrAdd(new TestKey(3), t => 3);
            cache.GetOrAdd(new TestKey(1), t => 1);
            cache.GetOrAdd(new TestKey(7), t => 7);
            cache.GetOrAdd(new TestKey(4), t => 4);
            cache.GetOrAdd(new TestKey(6), t => 6);
            cache.GetOrAdd(new TestKey(2), t => 2);
            cache.GetOrAdd(new TestKey(8), t => 8);
            cache.GetOrAdd(new TestKey(5), t => 5);

            cache.Count.ShouldBe(8);

            var three = cache.GetOrAdd(new TestKey(3), null);

            three.ShouldBe(3);

            var five = cache.GetOrAdd(new TestKey(5), null);

            five.ShouldBe(5);

            var six = cache.GetOrAdd(new TestKey(6), null);

            six.ShouldBe(6);
        }

        [Fact]
        public void ShouldRetrieveFromAnOddNumberedLengthCache()
        {
            ICache<TestKey, int> cache = new HashCodeArrayCache<TestKey, int>();

            cache.GetOrAdd(new TestKey(3), t => 3);
            cache.GetOrAdd(new TestKey(1), t => 1);
            cache.GetOrAdd(new TestKey(7), t => 7);
            cache.GetOrAdd(new TestKey(4), t => 4);
            cache.GetOrAdd(new TestKey(6), t => 6);

            cache.Count.ShouldBe(5);

            var three = cache.GetOrAdd(new TestKey(3), null);

            three.ShouldBe(3);

            var six = cache.GetOrAdd(new TestKey(6), null);

            six.ShouldBe(6);
        }

        private struct TestKey
        {
            private readonly int _hashCode;

            public TestKey(int hashCode)
            {
                _hashCode = hashCode;
            }

            public override int GetHashCode() => _hashCode;
        }
    }
}
