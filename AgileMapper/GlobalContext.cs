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
            MemberCache = new MemberCache(Cache);
            DerivedTypes = new DerivedTypesCache(Cache);
        }

        public CacheSet Cache { get; }

        public MemberCache MemberCache { get; }

        public DerivedTypesCache DerivedTypes { get; }
    }
}