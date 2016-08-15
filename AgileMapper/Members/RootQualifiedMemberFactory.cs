namespace AgileObjects.AgileMapper.Members
{
    using Caching;

    internal class RootQualifiedMemberFactory
    {
        private readonly NamingSettings _namingSettings;
        private readonly ICache<QualifiedMemberKey, IQualifiedMember> _memberCache;

        public RootQualifiedMemberFactory(MapperContext mapperContext)
        {
            _namingSettings = mapperContext.NamingSettings;
            _memberCache = mapperContext.Cache.Create<QualifiedMemberKey, IQualifiedMember>();
        }

        public IQualifiedMember RootSource<TSource>()
        {
            var memberKey = QualifiedMemberKey.ForSource<TSource>();

            var rootMember = _memberCache.GetOrAdd(
                memberKey,
                k => QualifiedMember.From(Member.RootSource<TSource>(), _namingSettings));

            return rootMember;
        }

        public QualifiedMember RootTarget<TTarget>()
        {
            var memberKey = QualifiedMemberKey.ForTarget<TTarget>();

            var rootMember = _memberCache.GetOrAdd(
                memberKey,
                k => QualifiedMember.From(Member.RootTarget<TTarget>(), _namingSettings));

            return (QualifiedMember)rootMember;
        }

        private class QualifiedMemberKey
        {
            public static QualifiedMemberKey ForSource<TSource>() => SourceKey<TSource>.Instance;

            public static QualifiedMemberKey ForTarget<TTarget>() => TargetKey<TTarget>.Instance;

            // ReSharper disable once UnusedTypeParameter
            private static class SourceKey<T>
            {
                // ReSharper disable once StaticMemberInGenericType
                public static readonly QualifiedMemberKey Instance = new QualifiedMemberKey();
            }

            // ReSharper disable once UnusedTypeParameter
            private static class TargetKey<T>
            {
                // ReSharper disable once StaticMemberInGenericType
                public static readonly QualifiedMemberKey Instance = new QualifiedMemberKey();
            }
        }
    }
}