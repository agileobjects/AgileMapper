namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Linq.Expressions;
    using Extensions;
    using ObjectPopulation;

    public interface IMappingData<out TSource, TTarget>
    {
        IMappingData Parent { get; }

        TSource Source { get; }

        TTarget Target { get; set; }

        int? EnumerableIndex { get; }
    }

    public interface IMappingData
    {
        IMappingData Parent { get; }

        TSource GetSource<TSource>();

        TTarget GetTarget<TTarget>();

        int? GetEnumerableIndex();
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

    internal class MappingInstanceData<TSource, TTarget>
    {
        public MappingInstanceData(
            MappingContext mappingContext,
            TSource source,
            TTarget target,
            int? enumerableIndex = null)
        {
            MappingContext = mappingContext;
            Source = source;
            Target = target;
            EnumerableIndex = enumerableIndex;
        }

        public MappingContext MappingContext { get; }

        public TSource Source { get; }

        public TTarget Target { get; set; }

        public int? EnumerableIndex { get; }
    }

    internal class MappingData<TSource, TTarget> :
        MappingInstanceData<TSource, TTarget>,
        IMappingData,
        IMappingData<TSource, TTarget>,
        IMemberMapperCreationData,
        IObjectMapperCreationData
    {
        private readonly MemberMapperData _memberMapperData;

        public MappingData(MappingInstanceData<TSource, TTarget> instanceData, ObjectMapperData mapperData)
            : this(instanceData, mapperData, parent: null)
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
                  instanceData.EnumerableIndex)
        {
            _memberMapperData = mapperData;
            Parent = parent;
        }

        public ObjectMapperData MapperData { get; }

        public IMappingData Parent { get; }

        public TTarget CreatedObject { get; set; }

        #region IMappingData Members

        T IMappingData.GetSource<T>() => (T)(object)Source;

        T IMappingData.GetTarget<T>() => (T)(object)Target;

        int? IMappingData.GetEnumerableIndex() => EnumerableIndex ?? Parent?.GetEnumerableIndex();

        #endregion

        #region IMappingData<TSource, TTarget> Members

        IMappingData IMappingData<TSource, TTarget>.Parent => Parent;

        #endregion

        #region IMapperCreationData Members

        MappingRuleSet IMapperCreationData.RuleSet => MapperData.RuleSet;

        IQualifiedMember IMapperCreationData.SourceMember => MapperData.SourceMember;

        QualifiedMember IMapperCreationData.TargetMember => MapperData.TargetMember;

        #endregion

        #region IMemberMapperCreationData Members

        MemberMapperData IMemberMapperCreationData.MapperData => _memberMapperData;

        Type IMemberMapperCreationData.GetSourceMemberRuntimeType(IQualifiedMember sourceMember)
        {
            if (Source == null)
            {
                return sourceMember.Type;
            }

            if (sourceMember == MapperData.SourceMember)
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
                var relativeMember = sourceMember.RelativeTo(MapperData.SourceMember);
                var memberAccess = relativeMember.GetQualifiedAccess(MapperData.SourceObject);
                memberAccess = memberAccess.Replace(MapperData.SourceObject, sourceParameter);

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
            MappingData<TDeclaredSource, TDeclaredTarget> data,
            string targetMemberName,
            int dataSourceIndex)
        {
            var childMapperDataBridge = MapperData.CreateChildMapperDataBridge(
                data,
                targetMemberName,
                dataSourceIndex);

            return MapChild(data, childMapperDataBridge);
        }

        public TTargetElement Map<TSourceElement, TTargetElement>(
            MappingInstanceData<TSourceElement, TTargetElement> data)
        {
            var elementMapperDataBridge = MapperData.CreateElementMapperDataBridge(data);

            return MapChild(data, elementMapperDataBridge);
        }

        private TChildTarget MapChild<TChildSource, TChildTarget>(
            MappingInstanceData<TChildSource, TChildTarget> instanceData,
            IObjectMappingContextFactoryBridge mapperDataBridge)
        {
            var elementMapperData = mapperDataBridge.ToMapperData();

            return MappingContext.MapChild(instanceData, elementMapperData);
        }
    }
}