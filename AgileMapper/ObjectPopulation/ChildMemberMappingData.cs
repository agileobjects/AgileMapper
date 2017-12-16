namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using Caching;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;

    internal class ChildMemberMappingData<TSource, TTarget> : IChildMemberMappingData
    {
        private readonly ObjectMappingData<TSource, TTarget> _parent;
        private readonly ICache<IQualifiedMember, Func<TSource, Type>> _runtimeTypeGettersCache;

        public ChildMemberMappingData(ObjectMappingData<TSource, TTarget> parent, IMemberMapperData mapperData)
        {
            _parent = parent;
            _runtimeTypeGettersCache = parent.MapperContext.Cache.CreateScoped<IQualifiedMember, Func<TSource, Type>>();
            MapperData = mapperData;
        }

        public MappingRuleSet RuleSet => _parent.MappingContext.RuleSet;

        public IObjectMappingData Parent => _parent;

        public IMemberMapperData MapperData { get; }

        public Type GetSourceMemberRuntimeType(IQualifiedMember sourceMember)
        {
            if (_parent.Source == null)
            {
                return sourceMember.Type;
            }

            if (sourceMember.Type.IsSealed())
            {
                return sourceMember.Type;
            }

            var mapperData = MapperData;

            while (mapperData != null)
            {
                if (sourceMember == mapperData.SourceMember)
                {
                    return mapperData.SourceMember.Type;
                }

                mapperData = mapperData.Parent;
            }

            var getRuntimeTypeFunc = _runtimeTypeGettersCache.GetOrAdd(sourceMember, sm =>
            {
                var sourceParameter = Parameters.Create<TSource>("source");
                var relativeMember = sm.RelativeTo(MapperData.SourceMember);
                var memberAccess = relativeMember.GetQualifiedAccess(MapperData);
                memberAccess = memberAccess.Replace(MapperData.SourceObject, sourceParameter);

                var getRuntimeTypeCall = Expression.Call(
                    ObjectExtensions.GetRuntimeSourceTypeMethod.MakeGenericMethod(sm.Type),
                    memberAccess);

                var getRuntimeTypeLambda = Expression
                    .Lambda<Func<TSource, Type>>(getRuntimeTypeCall, sourceParameter);

                return getRuntimeTypeLambda.Compile();
            });

            return getRuntimeTypeFunc.Invoke(_parent.Source);
        }
    }
}