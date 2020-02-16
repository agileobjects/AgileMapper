namespace AgileObjects.AgileMapper.Members
{
    using Caching;
    using Dictionaries;
    using ReadableExpressions.Extensions;

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
                QualifiedMemberKey.Keys<TSource, TTarget>.Source,
                k =>
                {
                    var matchedTargetMember = RootTarget<TSource, TTarget>();

                    var sourceMember = QualifiedMember
                        .CreateRoot(Member.RootSource<TSource>(), _mapperContext)
                        .SetContext(matchedTargetMember.Context);

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
                QualifiedMemberKey.Keys<TSource, TTarget>.Target,
                k =>
                {
                    var targetMember = QualifiedMember.CreateRoot(Member.RootTarget<TTarget>(), _mapperContext);

                    SetContext<TSource, TTarget>(targetMember);

                    return GetFinalTargetMember(targetMember);
                });

            return (QualifiedMember)rootMember;
        }

        private void SetContext<TSource, TTarget>(QualifiedMember targetMember)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new QualifiedMemberContext(
                MappingRuleSet.All,
                typeof(TSource),
                typeof(TTarget),
                targetMember,
                parent: null,
                mapperContext: _mapperContext);
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
            // ReSharper disable UnusedTypeParameter
            // ReSharper disable StaticMemberInGenericType
            public static class Keys<T1, T2>
            {
                public static readonly QualifiedMemberKey Source = new QualifiedMemberKey();
                public static readonly QualifiedMemberKey Target = new QualifiedMemberKey();
            }
            // ReSharper restore StaticMemberInGenericType
            // ReSharper restore UnusedTypeParameter
        }

        #endregion
    }
}