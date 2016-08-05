namespace AgileObjects.AgileMapper
{
    using System;
    using Caching;
    using Members;

    internal class GlobalContext
    {
        public static readonly GlobalContext Instance = new GlobalContext();

        private readonly Lazy<MemberFinder> _memberFinderLoader;

        private GlobalContext()
        {
            Cache = new CacheSet(this);
            _memberFinderLoader = new Lazy<MemberFinder>(() => new MemberFinder(this), isThreadSafe: true);
        }

        public CacheSet Cache { get; }

        public MemberFinder MemberFinder => _memberFinderLoader.Value;

        public ICache<TKey, TValue> CreateCache<TKey, TValue>() => new DictionaryCache<TKey, TValue>();
    }
}