namespace AgileObjects.AgileMapper.Members
{
    using Caching;
    using Dictionaries;
    using Extensions.Internal;

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
                QualifiedMemberKey.ForSource<TSource, TTarget>(),
                k =>
                {
                    var sourceMember = QualifiedMember.From(Member.RootSource<TSource>(), _mapperContext);
                    var matchedTargetMember = RootTarget<TSource, TTarget>();

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

        public QualifiedMember RootTarget<TSource, TTarget>()
        {
            var rootMember = _memberCache.GetOrAdd(
                QualifiedMemberKey.ForTarget<TSource, TTarget>(),
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

        #region Key Class

        private class QualifiedMemberKey
        {
            public static QualifiedMemberKey ForSource<TSource, TTarget>() => SourceKey<TSource, TTarget>.Instance;

            public static QualifiedMemberKey ForTarget<TSource, TTarget>() => TargetKey<TSource, TTarget>.Instance;

            // ReSharper disable UnusedTypeParameter
            // ReSharper disable StaticMemberInGenericType
            private static class SourceKey<T1, T2>
            {
                public static readonly QualifiedMemberKey Instance = new QualifiedMemberKey();
            }

            private static class TargetKey<T1, T2>
            {
                public static readonly QualifiedMemberKey Instance = new QualifiedMemberKey();
            }
            // ReSharper restore StaticMemberInGenericType
            // ReSharper restore UnusedTypeParameter
        }

        #endregion
    }
}