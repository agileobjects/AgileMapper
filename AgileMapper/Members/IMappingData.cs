namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Linq.Expressions;
    using Extensions;
    using ObjectPopulation;

    public interface IMappingData<out TSource, out TTarget>
    {
        IMappingData Parent { get; }

        TSource Source { get; }

        TTarget Target { get; }

        int? EnumerableIndex { get; }
    }

    public interface IMappingData
    {
        IMappingData Parent { get; }

        TSource GetSource<TSource>();

        TTarget GetTarget<TTarget>();

        int? GetEnumerableIndex();

        IMappingData<TParentSource, TParentTarget> Typed<TParentSource, TParentTarget>();
    }

    internal interface IMapperCreationData
    {
        MappingRuleSet RuleSet { get; }

        IQualifiedMember SourceMember { get; }

        QualifiedMember TargetMember { get; }
    }

    internal interface IMemberMapperCreationData : IMapperCreationData
    {
        MemberMapperData MapperData { get; }

        Type GetSourceMemberRuntimeType(IQualifiedMember sourceMember);
    }

    internal interface IObjectMapperCreationData : IMapperCreationData
    {
        ObjectMapperData MapperData { get; }

        IMemberMapperCreationData GetChildCreationData(MemberMapperData childMapperData);
    }

    internal class MappingInstanceData<TSource, TTarget> : IMappingData<TSource, TTarget>
    {
        public MappingInstanceData(
            MappingContext mappingContext,
            TSource source,
            TTarget target,
            int? enumerableIndex = null,
            IMappingData parent = null)
        {
            MappingContext = mappingContext;
            Parent = parent;
            Source = source;
            Target = target;
            EnumerableIndex = enumerableIndex;
        }

        public MappingContext MappingContext { get; }

        public IMappingData Parent { get; }

        public TSource Source { get; }

        public TTarget Target { get; set; }

        public int? EnumerableIndex { get; }
    }

    internal class MappingData<TSource, TTarget> :
        MappingInstanceData<TSource, TTarget>,
        IMappingData,
        IMemberMapperCreationData,
        IObjectMapperCreationData
    {
        private readonly MemberMapperData _memberMapperData;

        public MappingData(
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

        public MappingData(MappingInstanceData<TSource, TTarget> instanceData, ObjectMapperData mapperData)
            : this(instanceData, mapperData, instanceData.Parent)
        {
            MapperData = mapperData;
        }

        private MappingData(
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
            _memberMapperData = mapperData;
        }

        public ObjectMapperData MapperData { get; }

        public TTarget CreatedObject { get; set; }

        #region IMappingData Members

        T IMappingData.GetSource<T>() => (T)(object)Source;

        T IMappingData.GetTarget<T>() => (T)(object)Target;

        public int? GetEnumerableIndex() => EnumerableIndex ?? Parent?.GetEnumerableIndex();

        IMappingData<TParentSource, TParentTarget> IMappingData.Typed<TParentSource, TParentTarget>()
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

            var getRuntimeTypeFunc = GlobalContext.Instance.Cache.GetOrAdd(accessKey, k =>
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
            => new MappingData<TSource, TTarget>(this, childMapperData, this);

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
                instanceData => MapperData.CreateChildMappingDataBridge(
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
                instanceData => MapperData.CreateElementMappingDataBridge(instanceData));
        }

        private TChildTarget MapChild<TChildSource, TChildTarget>(
            TChildSource sourceValue,
            TChildTarget targetValue,
            int? enumerableIndex,
            Func<MappingInstanceData<TChildSource, TChildTarget>, IMappingDataFactoryBridge> bridgeFactory)
        {
            var instanceData = new MappingInstanceData<TChildSource, TChildTarget>(
                MappingContext,
                sourceValue,
                targetValue,
                enumerableIndex,
                this);

            var mappingDataBridge = bridgeFactory.Invoke(instanceData);
            var mappingData = mappingDataBridge.GetMapperCreationData();

            return MappingContext.MapChild<TChildSource, TChildTarget>(mappingData);
        }
    }
}