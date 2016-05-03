namespace AgileObjects.AgileMapper
{
    using Caching;
    using Members;

    internal class GlobalContext
    {
        public static readonly GlobalContext Default = new GlobalContext();

        private static readonly object _memberFinderSyncLock = new object();
        private static readonly object _cacheSyncLock = new object();

        private ICache _cache;
        private MemberFinder _memberFinder;

        private GlobalContext()
        {
        }

        public ICache Cache
        {
            get
            {
                if (_cache == null)
                {
                    lock (_cacheSyncLock)
                    {
                        if (_cache == null)
                        {
                            _cache = CreateCache();
                        }
                    }
                }

                return _cache;
            }
        }

        public MemberFinder MemberFinder
        {
            get
            {
                if (_memberFinder == null)
                {
                    lock (_memberFinderSyncLock)
                    {
                        if (_memberFinder == null)
                        {
                            _memberFinder = new MemberFinder(Cache);
                        }
                    }
                }

                return _memberFinder;
            }
        }

        public ICache CreateCache()
        {
            return new DictionaryCache();
        }
    }
}