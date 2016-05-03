namespace AgileObjects.AgileMapper
{
    using Caching;
    using Members;

    internal class GlobalContext
    {
        public static readonly GlobalContext Default = new GlobalContext();

        private static readonly object _syncLock = new object();

        private MemberFinder _memberFinder;

        private GlobalContext()
        {
        }

        public MemberFinder MemberFinder
        {
            get
            {
                if (_memberFinder == null)
                {
                    lock (_syncLock)
                    {
                        if (_memberFinder == null)
                        {
                            _memberFinder = new MemberFinder(CreateCache());
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