namespace AgileObjects.AgileMapper.Members
{
    using Caching;
    using Extensions;

    internal class RootQualifiedMemberFactory
    {
        private readonly MapperContext _mapperContext;
        private readonly ICache<QualifiedMemberKey, IQualifiedMember> _memberCache;

        public RootQualifiedMemberFactory(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
            _memberCache = mapperContext.Cache.CreateScoped<QualifiedMemberKey, IQualifiedMember>();
        }

        public IQualifiedMember RootSource<TSource, TTarget>()
        {
            var memberKey = QualifiedMemberKey.ForSource<TSource>();

            var rootMember = _memberCache.GetOrAdd(
                memberKey,
                k =>
                {
                    var sourceMember = QualifiedMember.From(Member.RootSource<TSource>(), _mapperContext);

                    if (typeof(TSource).IsDictionary())
                    {
                        return new DictionarySourceMember(sourceMember, RootTarget<TTarget>());
                    }

                    return sourceMember;
                });

            return rootMember;
        }

        public QualifiedMember RootTarget<TTarget>()
        {
            var memberKey = QualifiedMemberKey.ForTarget<TTarget>();

            var rootMember = _memberCache.GetOrAdd(
                memberKey,
                k =>
                {
                    var targetMember = QualifiedMember.From(Member.RootTarget<TTarget>(), _mapperContext);

                    if (typeof(TTarget).IsDictionary())
                    {
                        return new DictionaryTargetMember(targetMember);
                    }

                    return targetMember;
                });

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