namespace AgileObjects.AgileMapper.UnitTests.Caching
{
    using System;
    using System.Linq;
#if !NET35
    using System.Threading.Tasks;
#endif
    using AgileMapper.Caching;
    using Common;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenAccessingCaches
    {
        [Fact]
        public void ShouldAcceptANullKey()
        {
            var cache = new DefaultArrayCache<string, int>(keyComparer: null);
            cache.GetOrAdd(null, str => 123);

            cache.GetOrAdd(null, str => 456).ShouldBe(123);
        }

        // See https://github.com/agileobjects/AgileMapper/issues/212#issuecomment-812121113
        [Fact]
        public void ShouldHandleANullKey()
        {
            var cache = new DefaultArrayCache<string, int>(keyComparer: null);
            cache.GetOrAdd(null, str => 123);
            cache.GetOrAdd("456", str => 456);

            cache.GetOrAdd(null, str => 456).ShouldBe(123);
        }

#if !NET35
        [Fact]
        public async Task ShouldBeThreadSafe()
        {
            const int CACHE_COUNT = 10;
            var random = new Random();

            var caches = Enumerable
                .Range(1, CACHE_COUNT)
                .Select(i => i % 2 != 0
                    ? new DefaultArrayCache<int, string>(keyComparer: null)
                    : (ICache<int, string>)new HashCodeArrayCache<int, string>())
                .ToList();

            var cachingTasks = Enumerable
                .Range(1, 1_000_000)
                .Select(i => Task.Run(() =>
                {
                    // Select a cache at random:
                    var cache = caches[random.Next(minValue: 0, maxValue: CACHE_COUNT)];

                    // Select a key at random - this should produce a mixture
                    // of cache hits and misses. Default cache capacity is 10,
                    // so almost certain all caches will resize during the test:
                    var key = random.Next(minValue: 1, maxValue: 50);

                    var cachedValue = cache.GetOrAdd(key, k => k.ToString());
                    cachedValue.ShouldBe(key.ToString());
                }));

            await Task.WhenAll(cachingTasks);

            caches.All(cache => cache.Count > 0).ShouldBeTrue();
        }
#endif
    }
}
