namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class ObjectMappingData<TSource, TTarget> :
        MappingInstanceData<TSource, TTarget>,
        IMappingData,
        IMemberMapperCreationData,
        IObjectMapperCreationData
    {
        private readonly MemberMapperData _memberMapperData;

        // Called by MapperCreationDataFactory:
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

            var getRuntimeTypeFunc = GlobalContext.Instance.Cache.GetOrAdd(accessKey, k =>
            {
                ParameterExpression sourceParameter;
                var memberAccess = GetSourceMemberAccess(sourceMember, out sourceParameter);

                var getRuntimeTypeCall = Expression.Call(
                    ObjectExtensions.GetRuntimeSourceTypeMethod.MakeGenericMethod(sourceMember.Type),
                    memberAccess);

                var getRuntimeTypeLambda = Expression
                    .Lambda<Func<TSource, Type>>(getRuntimeTypeCall, sourceParameter);

                return getRuntimeTypeLambda.Compile();
            });

            return getRuntimeTypeFunc.Invoke(Source);
        }

        MappingInstanceData<TChildSource, TChildTarget> IMemberMapperCreationData
            .CreateChildMappingInstanceData<TChildSource, TChildTarget>(IQualifiedMember sourceMember)
        {
            var key = new SourceAndTargetMembersKey(sourceMember, _memberMapperData.TargetMember);

            var instanceDataCreationFunc = GlobalContext.Instance.Cache.GetOrAdd(key, k =>
            {
                ParameterExpression sourceParameter;
                var sourceMemberAccess = GetSourceMemberAccess(sourceMember, out sourceParameter);

                var targetParameter = Parameters.Create<TTarget>("target");
                var targetMemberAccess = _memberMapperData.GetTargetMemberAccess(targetParameter);

                var mappingDataParameter = Parameters.Create<IMappingData>("mappingData");

                var instanceDataCreation = Expression.New(
                    typeof(MappingInstanceData<TChildSource, TChildTarget>).GetConstructors().First(),
                    Parameters.MappingContext,
                    sourceMemberAccess,
                    targetMemberAccess,
                    Parameters.EnumerableIndexNullable,
                    mappingDataParameter);

                var instanceDataCreationLambda = Expression
                    .Lambda<Func<MappingContext, TSource, TTarget, int?, IMappingData, MappingInstanceData<TChildSource, TChildTarget>>>(
                        instanceDataCreation,
                        Parameters.MappingContext,
                        sourceParameter,
                        targetParameter,
                        Parameters.EnumerableIndexNullable,
                        mappingDataParameter);

                return instanceDataCreationLambda.Compile();
            });

            return instanceDataCreationFunc.Invoke(
                MappingContext,
                Source,
                Target,
                GetEnumerableIndex(),
                this);
        }

        private Expression GetSourceMemberAccess(IQualifiedMember sourceMember, out ParameterExpression sourceParameter)
        {
            sourceParameter = Parameters.Create<TSource>("source");
            var memberAccess = _memberMapperData.GetSourceMemberAccess(sourceMember, sourceParameter);

            return memberAccess;
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

        public ObjectMappingData<TChildSource, TChildTarget> CreateChildMappingData<TChildSource, TChildTarget>(
            TChildSource childSource,
            TChildTarget childTarget,
            string targetMemberName,
            int dataSourceIndex)
        {
            var childMapperData = MapperData.CreateChildMapperData(
                MappingContext,
                targetMemberName,
                dataSourceIndex);

            var childInstanceData = new MappingInstanceData<TChildSource, TChildTarget>(
                MappingContext,
                childSource,
                childTarget,
                GetEnumerableIndex(),
                this);

            var childMappingData = new ObjectMappingData<TChildSource, TChildTarget>(
                childInstanceData,
                childMapperData,
                this);

            return childMappingData;
        }
    }
}