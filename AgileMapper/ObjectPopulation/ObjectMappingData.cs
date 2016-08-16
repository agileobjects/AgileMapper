namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using Caching;
    using Extensions;
    using Members;

    internal class ObjectMappingData<TSource, TTarget> :
        MappingInstanceData<TSource, TTarget>,
        IMappingData,
        IMemberMapperCreationData,
        IObjectMapperCreationData
    {
        private readonly ICache<string, Func<TSource, Type>> _runtimeTypeGettersCache;
        private readonly MemberMapperData _memberMapperData;

        public ObjectMappingData(
            MappingContext mappingContext,
            TSource source,
            TTarget target,
            int? enumerableIndex,
            ObjectMapperData mapperData)
            : this(
                new MappingInstanceData<TSource, TTarget>(
                    mappingContext,
                    source,
                    target,
                    enumerableIndex),
                mapperData)
        {
        }

        public ObjectMappingData(MappingInstanceData<TSource, TTarget> instanceData, ObjectMapperData mapperData)
            : this(instanceData, mapperData, instanceData.Parent)
        {
            MapperData = mapperData;
        }

        private ObjectMappingData(
            MappingInstanceData<TSource, TTarget> instanceData,
            MemberMapperData mapperData,
            IMappingData parent)
            : base(
                instanceData.MappingContext,
                instanceData.Source,
                instanceData.Target,
                instanceData.EnumerableIndex,
                parent)
        {
            _runtimeTypeGettersCache = GlobalContext.Instance.Cache.CreateScoped<string, Func<TSource, Type>>();
            _memberMapperData = mapperData;
        }

        public ObjectMapperData MapperData { get; }

        public TTarget CreatedObject { get; set; }

        #region IMappingData Members

        T IMappingData.GetSource<T>() => (T)(object)Source;

        T IMappingData.GetTarget<T>() => (T)(object)Target;

        public int? GetEnumerableIndex() => EnumerableIndex ?? Parent?.GetEnumerableIndex();

        IMappingData<TParentSource, TParentTarget> IMappingData.As<TParentSource, TParentTarget>()
            => (IMappingData<TParentSource, TParentTarget>)this;

        #endregion

        #region IMapperCreationData Members

        MappingRuleSet IMapperCreationData.RuleSet => _memberMapperData.RuleSet;

        IQualifiedMember IMapperCreationData.SourceMember => _memberMapperData.SourceMember;

        QualifiedMember IMapperCreationData.TargetMember => _memberMapperData.TargetMember;

        #endregion

        #region IMemberMapperCreationData Members

        MemberMapperData IMemberMapperCreationData.MapperData => _memberMapperData;

        Type IMemberMapperCreationData.GetSourceMemberRuntimeType(IQualifiedMember sourceMember)
        {
            if (Source == null)
            {
                return sourceMember.Type;
            }

            if (sourceMember == _memberMapperData.SourceMember)
            {
                return sourceMember.Type;
            }

            if (sourceMember.Type.IsSealed)
            {
                return sourceMember.Type;
            }

            var accessKey = sourceMember.Signature + ": GetRuntimeSourceType";

            var getRuntimeTypeFunc = _runtimeTypeGettersCache.GetOrAdd(accessKey, k =>
            {
                var sourceParameter = Parameters.Create<TSource>("source");
                var relativeMember = sourceMember.RelativeTo(_memberMapperData.SourceMember);
                var memberAccess = relativeMember.GetQualifiedAccess(_memberMapperData.SourceObject);
                memberAccess = memberAccess.Replace(_memberMapperData.SourceObject, sourceParameter);

                var getRuntimeTypeCall = Expression.Call(
                    ObjectExtensions.GetRuntimeSourceTypeMethod.MakeGenericMethod(sourceMember.Type),
                    memberAccess);

                var getRuntimeTypeLambda = Expression
                    .Lambda<Func<TSource, Type>>(getRuntimeTypeCall, sourceParameter);

                return getRuntimeTypeLambda.Compile();
            });

            return getRuntimeTypeFunc.Invoke(Source);
        }

        #endregion

        #region IObjectMapperCreationData Members

        IMemberMapperCreationData IObjectMapperCreationData.GetChildCreationData(MemberMapperData childMapperData)
            => new ObjectMappingData<TSource, TTarget>(this, childMapperData, this);

        #endregion

        public TDeclaredTarget Map<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceValue,
            TDeclaredTarget targetValue,
            string targetMemberName,
            int dataSourceIndex)
        {
            return MapChild(
                sourceValue,
                targetValue,
                GetEnumerableIndex(),
                instanceData => MapperData.CreateChildMapperCreationData(
                    instanceData,
                    targetMemberName,
                    dataSourceIndex));
        }

        public TTargetElement Map<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int enumerableIndex)
        {
            return MapChild(
                sourceElement,
                targetElement,
                enumerableIndex,
                instanceData => MapperData.CreateElementMapperCreationData(instanceData));
        }

        private TChildTarget MapChild<TChildSource, TChildTarget>(
            TChildSource sourceValue,
            TChildTarget targetValue,
            int? enumerableIndex,
            Func<MappingInstanceData<TChildSource, TChildTarget>, IObjectMapperCreationData> creationDataFactory)
        {
            var instanceData = new MappingInstanceData<TChildSource, TChildTarget>(
                MappingContext,
                sourceValue,
                targetValue,
                enumerableIndex,
                this);

            var mapperCreationData = creationDataFactory.Invoke(instanceData);

            return MappingContext.Map<TChildSource, TChildTarget>(mapperCreationData);
        }
    }
}