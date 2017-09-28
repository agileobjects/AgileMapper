namespace AgileObjects.AgileMapper
{
    using Caching;
    using Members;

    internal class GlobalContext
    {
        public static readonly GlobalContext Instance = new GlobalContext();

        private GlobalContext()
        {
            Cache = new CacheSet();
            MemberFinder = new MemberFinder(Cache);
            DerivedTypes = new DerivedTypesCache(Cache);
        }

        public CacheSet Cache { get; }

        public MemberFinder MemberFinder { get; }

        public DerivedTypesCache DerivedTypes { get; }
    }
}