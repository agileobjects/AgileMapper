namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Members;

    internal class ObjectMappingContextData<TSource, TTarget> : BasicMappingContextData<TSource, TTarget>,
        IMappingContextData,
        IObjectMappingContextData,
        IObjectMapperKey
    {
        private readonly IObjectMappingContextData _parent;

        public ObjectMappingContextData(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            bool runtimeTypesAreTheSame,
            IMappingContext mappingContext,
            IObjectMappingContextData parent = null)
            : base(source, target, enumerableIndex, sourceMember, targetMember, mappingContext.RuleSet, parent)
        {
            _parent = parent;
            MappingContext = mappingContext;
            RuntimeTypesAreTheSame = runtimeTypesAreTheSame;
            MapperData = GetObjectMapperData();
        }

        #region Setup

        private ObjectMapperData GetObjectMapperData()
        {
            var mapperData = MapperContext.Cache.GetOrAdd(
                (IObjectMapperKey)this,
                key => new ObjectMapperData(
                    MappingContext,
                    SourceMember,
                    TargetMember,
                    _parent?.MapperData),
                key =>
                {
                    var data = (ObjectMappingContextData<TSource, TTarget>)key;

                    data.MapperKeyObject = new ObjectMapperKey(
                        data.RuleSet,
                        data.SourceMember,
                        data.TargetMember);

                    return data.MapperKeyObject;
                });

            return mapperData;
        }

        #endregion

        public IMappingContext MappingContext { get; }

        public MapperContext MapperContext => MappingContext.MapperContext;

        public TTarget CreatedObject { get; set; }

        public bool RuntimeTypesAreTheSame { get; }

        public ObjectMapperData MapperData { get; }

        #region IObjectMapperKey Members

        public ObjectMapperKey MapperKeyObject { get; private set; }

        public void AddSourceMemberTypeTester(Func<IMappingData, bool> tester)
            => MapperKeyObject.AddSourceMemberTypeTester(tester);

        bool IObjectMapperKey.SourceHasRequiredTypes(IMappingData data)
            => MapperKeyObject.SourceHasRequiredTypes(data);

        #endregion

        #region IMappingData Members

        T IMappingData.GetSource<T>() => (T)(object)Source;

        T IMappingData.GetTarget<T>() => (T)(object)Target;

        public int? GetEnumerableIndex() => EnumerableIndex ?? Parent?.GetEnumerableIndex();

        IMappingData<TDataSource, TDataTarget> IMappingData.As<TDataSource, TDataTarget>()
            => (IMappingData<TDataSource, TDataTarget>)this;

        #endregion

        #region IObjectMappingContextData Members

        private MemberMappingContextData<TSource, TTarget> _childContextData;

        IMemberMappingContextData IObjectMappingContextData.GetChildContextData(MemberMapperData childMapperData)
        {
            if (_childContextData == null)
            {
                _childContextData = new MemberMappingContextData<TSource, TTarget>(this);
            }

            _childContextData.MapperData = childMapperData;

            return _childContextData;
        }

        #endregion

        #region Map Methods

        public TDeclaredTarget Map<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceValue,
            TDeclaredTarget targetValue,
            string targetMemberName,
            int dataSourceIndex)
        {
            var childContextData = ObjectMappingContextDataFactory.ForChild(
                sourceValue,
                targetValue,
                GetEnumerableIndex(),
                targetMemberName,
                dataSourceIndex,
                this);

            return MappingContext.Map<TDeclaredSource, TDeclaredTarget>(childContextData);
        }

        public TTargetElement Map<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int enumerableIndex)
        {
            var elementContextData = ObjectMappingContextDataFactory.ForElement(
                sourceElement,
                targetElement,
                enumerableIndex,
                this);

            return MappingContext.Map<TSourceElement, TTargetElement>(elementContextData);
        }

        #endregion
    }
}