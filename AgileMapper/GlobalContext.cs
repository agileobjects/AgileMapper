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
            _cacheLoader = new Lazy<ICache>(CreateCache, isThreadSafe: false);
            _memberFinderLoader = new Lazy<MemberFinder>(() => new MemberFinder(Cache), isThreadSafe: false);
        }

        public ICache Cache
        {
            get
            {
                // Changing this to an expression body 'inlines' it, which 
                // causes the MemberFinder to be passed a null ICache:
                return _cacheLoader.Value;
            }
        }

        public MemberFinder MemberFinder => _memberFinderLoader.Value;

        public ICache CreateCache()
        {
            return new DictionaryCache();
        }
    }
}