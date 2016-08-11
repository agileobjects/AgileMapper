namespace AgileObjects.AgileMapper
{
    using System;
    using Caching;
    using Members;

    internal class GlobalContext
    {
        public static readonly GlobalContext Instance = new GlobalContext();

        private readonly Lazy<CacheSet> _cacheSetLoader;
        private readonly Lazy<MemberFinder> _memberFinderLoader;

        private GlobalContext()
        {
            _cacheSetLoader = new Lazy<CacheSet>(() => new CacheSet(), isThreadSafe: true);
            _memberFinderLoader = new Lazy<MemberFinder>(() => new MemberFinder(), isThreadSafe: true);
        }

        public CacheSet Cache => _cacheSetLoader.Value;

        public MemberFinder MemberFinder => _memberFinderLoader.Value;

        public ICache<TKey, TValue> CreateCache<TKey, TValue>() => new DictionaryCache<TKey, TValue>();
    }
}