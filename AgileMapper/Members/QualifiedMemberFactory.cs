namespace AgileObjects.AgileMapper.Members
{
    using Caching;
    using Extensions;

    internal class QualifiedMemberFactory
    {
        private readonly MapperContext _mapperContext;
        private readonly ICache<QualifiedMemberKey, IQualifiedMember> _memberCache;

        public QualifiedMemberFactory(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
            _memberCache = mapperContext.Cache.CreateScoped<QualifiedMemberKey, IQualifiedMember>();
        }

        public IQualifiedMember RootSource<TSource, TTarget>()
        {
            var rootMember = _memberCache.GetOrAdd(
                QualifiedMemberKey.ForSource<TSource>(),
                k =>
                {
                    var sourceMember = QualifiedMember.From(Member.RootSource<TSource>(), _mapperContext);
                    var matchedTargetMember = RootTarget<TTarget>();

                    return GetFinalSourceMember(sourceMember, matchedTargetMember);
                });

            return rootMember;
        }

        public IQualifiedMember GetFinalSourceMember(
            IQualifiedMember sourceMember,
            QualifiedMember matchedTargetMember)
        {
            if (sourceMember.Type.IsDictionary())
            {
                return new DictionarySourceMember(sourceMember, matchedTargetMember);
            }

            return sourceMember;
        }

        public QualifiedMember RootTarget<TTarget>()
        {
            var rootMember = _memberCache.GetOrAdd(
                QualifiedMemberKey.ForTarget<TTarget>(),
                k =>
                {
                    var targetMember = QualifiedMember.From(Member.RootTarget<TTarget>(), _mapperContext);

                    return GetFinalTargetMember(targetMember);
                });

            return (QualifiedMember)rootMember;
        }

        public QualifiedMember GetFinalTargetMember(QualifiedMember targetMember)
        {
            if (targetMember.IsDictionary)
            {
                return new DictionaryTargetMember(targetMember);
            }

            return targetMember;
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