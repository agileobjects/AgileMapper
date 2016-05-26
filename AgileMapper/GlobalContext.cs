namespace AgileObjects.AgileMapper
{
    using System;
    using Caching;
    using Members;

    internal class GlobalContext
    {
        public static readonly GlobalContext Default = new GlobalContext();

        private readonly Lazy<ICache> _cacheLoader;
        private readonly Lazy<MemberFinder> _memberFinderLoader;

        private GlobalContext()
        {
            _cacheLoader = new Lazy<ICache>(CreateCache, isThreadSafe: true);
            _memberFinderLoader = new Lazy<MemberFinder>(() => new MemberFinder(Cache), isThreadSafe: true);
        }

        public ICache Cache => _cacheLoader.Value;

        public MemberFinder MemberFinder => _memberFinderLoader.Value;

        public ICache CreateCache() => new DictionaryCache();
    }
}