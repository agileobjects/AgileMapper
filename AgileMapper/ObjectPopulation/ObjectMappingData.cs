namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching;
    using DataSources;
    using Extensions.Internal;
    using MapperKeys;
    using Members;
    using Members.Extensions;
    using NetStandardPolyfills;
    using Validation;
#if NET35
    using static Microsoft.Scripting.Ast.Expression;
#else
    using static System.Linq.Expressions.Expression;
#endif

    internal class ObjectMappingData<TSource, TTarget> :
        MappingInstanceData<TSource, TTarget>,
        IObjectMappingData,
        IObjectMappingData<TSource, TTarget>,
        IObjectCreationMappingData<TSource, TTarget, TTarget>
    {
        private ICache<IQualifiedMember, Func<TSource, Type>> _runtimeTypeGettersCache;
        private ObjectMapper<TSource, TTarget> _mapper;
        private ObjectMapperData _mapperData;

        public ObjectMappingData(
            TSource source,
            TTarget target,
            int? elementIndex,
            object elementKey,
            MappingTypes mappingTypes,
            IMappingContext mappingContext,
            IObjectMappingData parent,
            bool createMapper = true)
            : this(
                  source,
                  target,
                  elementIndex,
                  elementKey,
                  mappingTypes,
                  mappingContext,
                  null,
                  parent,
                  createMapper)
        {
        }

        private ObjectMappingData(
            TSource source,
            TTarget target,
            int? elementIndex,
            object elementKey,
            MappingTypes mappingTypes,
            IMappingContext mappingContext,
            IObjectMappingData declaredTypeMappingData,
            IObjectMappingData parent,
            bool createMapper)
            : base(source, target, elementIndex, elementKey, parent, mappingContext)
        {
            MappingTypes = mappingTypes;
            MappingContext = mappingContext;
            DeclaredTypeMappingData = declaredTypeMappingData;

            if (parent != null)
            {
                Parent = parent;
                return;
            }

            if (createMapper)
            {
                // TODO:
                //_mapper = MapperContext.ObjectMapperFactory.GetOrCreateRoot(this, mappingContext);
            }
        }

        public IMappingContext MappingContext { get; }

        public MapperContext MapperContext => MappingContext.MapperContext;

        public MappingTypes MappingTypes { get; }

        public ObjectMapperKeyBase MapperKey { get; set; }

        public IRootMapperKey EnsureRootMapperKey()
        {
            // TODO:
            //MapperKey = MappingContext.RuleSet.RootMapperKeyFactory.Invoke(this);

            return (IRootMapperKey)MapperKey;
        }

        public IObjectMapper GetOrCreateMapper()
        {
            if (_mapper != null)
            {
                return _mapper;
            }

            _mapper = MapperContext.ObjectMapperFactory.Create(this);

            MapperKey.MappingData = null;
            MapperKey.KeyData = null;

            if (_mapper == null)
            {
                return null;
            }

            if (IsRoot && MapperContext.UserConfigurations.ValidateMappingPlans)
            {
                // TODO: Test coverage for validation of standalone child mappers
                MappingValidator.Validate(_mapper.MapperData);
            }

            StaticMapperCache<TSource, TTarget>.AddIfAppropriate(_mapper, this);

            return _mapper;
        }

        public void SetMapper(IObjectMapper mapper)
            => _mapper = (ObjectMapper<TSource, TTarget>)mapper;

        public bool MapperDataPopulated => (_mapperData ?? _mapper?.MapperData) != null;

        IMemberMapperData IDataSourceSetInfo.MapperData => MapperData;

        public ObjectMapperData MapperData
        {
            get => _mapperData ??= _mapper?.MapperData ?? ObjectMapperData.For(this);
            set => _mapperData = value;
        }

        public TTarget CreatedObject { get; set; }

        #region IObjectMappingData Members

        public bool IsRoot => Parent == null;

        public IObjectMappingData Parent { get; }

        IObjectMappingDataUntyped IObjectMappingData<TSource, TTarget>.Parent => Parent;

        public bool IsPartOfRepeatedMapping { get; set; }

        public bool IsPartOfDerivedTypeMapping => DeclaredTypeMappingData != null;

        public IObjectMappingData DeclaredTypeMappingData { get; }

        IChildMemberMappingData IObjectMappingData.GetChildMappingData(IMemberMapperData childMapperData)
            => new ChildMemberMappingData<TSource, TTarget>(this, childMapperData);

        public Type GetSourceMemberRuntimeType(IQualifiedMember childSourceMember)
        {
            if (Source == null)
            {
                return childSourceMember.Type;
            }

            if (childSourceMember.Type.IsSealed())
            {
                return childSourceMember.Type;
            }

            var mapperData = MapperData;

            while (mapperData != null)
            {
                if (childSourceMember == mapperData.SourceMember)
                {
                    return mapperData.SourceMember.Type;
                }

                mapperData = mapperData.Parent;
            }

            if (_runtimeTypeGettersCache == null)
            {
                _runtimeTypeGettersCache = MapperContext.Cache.CreateScoped<IQualifiedMember, Func<TSource, Type>>();
            }

            var getRuntimeTypeFunc = _runtimeTypeGettersCache.GetOrAdd(childSourceMember, sm =>
            {
                var sourceObject = MapperData.SourceObject;

                var sourceObjectIsNotParameter =
                    sourceObject.NodeType != ExpressionType.Parameter;

                var sourceParameter = sourceObjectIsNotParameter
                    ? typeof(TSource).GetOrCreateSourceParameter()
                    : (ParameterExpression)sourceObject;

                var memberAccess = sm
                    .GetRelativeQualifiedAccess(MapperData);

                if (sourceObjectIsNotParameter)
                {
                    memberAccess = memberAccess.Replace(
                        sourceObject,
                        sourceParameter,
                        ExpressionEvaluation.Equivalator);
                }

                if (memberAccess.NodeType == ExpressionType.Invoke)
                {
                    var runtimeTypeLambda =
                        Lambda<Func<TSource, Type>>(Constant(sm.Type), sourceParameter);

                    return runtimeTypeLambda.Compile();
                }

                var getRuntimeTypeCall = Call(
                    ObjectExtensions.GetRuntimeSourceTypeMethod.MakeGenericMethod(sm.Type),
                    memberAccess);

                var getRuntimeTypeLambda =
                    Lambda<Func<TSource, Type>>(getRuntimeTypeCall, sourceParameter);

                return getRuntimeTypeLambda.Compile();
            });

            return getRuntimeTypeFunc.Invoke(Source);
        }

        #endregion

        #region Map Methods

        public TTarget MapStart() => _mapper.Map(Source, Target, context: null); // TODO

        #endregion

        public IObjectMappingData<TNewSource, TTarget> WithSource<TNewSource>(TNewSource newSource)
            => With(newSource, Target, isForDerivedTypeMapping: false);

        public IObjectMappingData<TNewSource, TNewTarget> WithSourceType<TNewSource, TNewTarget>(bool isForDerivedTypeMapping)
            where TNewSource : class
        {
            return With(Source as TNewSource, default(TNewTarget), isForDerivedTypeMapping);
        }

        public IObjectMappingData<TNewSource, TNewTarget> WithTargetType<TNewSource, TNewTarget>(bool isForDerivedTypeMapping)
            where TNewTarget : class
        {
            return With(default(TNewSource), Target as TNewTarget, isForDerivedTypeMapping);
        }

        public IObjectMappingData WithToTargetSource(IQualifiedMember sourceMember)
        {
            var newSourceType = GetSourceMemberRuntimeType(sourceMember);

            var newSourceMappingData = UpdateTypes(
                newSourceType,
                MapperData.TargetType,
                IsPartOfDerivedTypeMapping);

            // TODO:
            //newSourceMappingData.MapperKey = MappingContext
            //    .RuleSet
            //    .RootMapperKeyFactory
            //    .Invoke(newSourceMappingData);

            var newSourceMapperData = newSourceMappingData.MapperData;

            newSourceMapperData.OriginalMapperData = MapperData;
            newSourceMapperData.Context.IsForToTargetMapping = true;

            return newSourceMappingData;
        }

        IObjectMappingData IObjectMappingData.WithDerivedTypes(Type derivedSourceType, Type derivedTargetType)
            => UpdateTypes(derivedSourceType, derivedTargetType, isForDerivedTypeMapping: true);

        private IObjectMappingData UpdateTypes(
            Type newSourceType,
            Type newTargetType,
            bool isForDerivedTypeMapping)
        {
            var typesKey = new SourceAndTargetTypesKey(newSourceType, newTargetType);

            var typedAsCaller = GlobalContext.Instance.Cache.GetOrAddWithHashCodes(typesKey, k =>
            {
                var mappingDataParameter = Parameters.Create<IObjectMappingData<TSource, TTarget>>("mappingData");
                var isForDerivedTypeParameter = Parameters.Create<bool>("isForDerivedType");

                var typesConversionCall = mappingDataParameter.GetAsCall(
                    isForDerivedTypeParameter,
                    k.SourceType,
                    k.TargetType);

                var typesConversionLambda = Lambda<Func<IObjectMappingData<TSource, TTarget>, bool, IObjectMappingDataUntyped>>(
                        typesConversionCall,
                        mappingDataParameter,
                        isForDerivedTypeParameter);

                return typesConversionLambda.Compile();
            });

            return (IObjectMappingData)typedAsCaller.Invoke(this, isForDerivedTypeMapping);
        }

        public IObjectMappingData<TNewSource, TNewTarget> As<TNewSource, TNewTarget>(bool isForDerivedTypeMapping)
            where TNewSource : class
            where TNewTarget : class
        {
            return With(Source as TNewSource, Target as TNewTarget, isForDerivedTypeMapping);
        }

        private IObjectMappingData<TNewSource, TNewTarget> With<TNewSource, TNewTarget>(
            TNewSource typedSource,
            TNewTarget typedTarget,
            bool isForDerivedTypeMapping)
        {
            if (MapperKey == null)
            {
                EnsureRootMapperKey();
            }

            var forceNewKey = isForDerivedTypeMapping && MapperKey.MappingTypes.TargetType.IsInterface();
            var mapperKey = MapperKey.WithTypes(typeof(TNewSource), typeof(TNewTarget), forceNewKey);

            return new ObjectMappingData<TNewSource, TNewTarget>(
                typedSource,
                typedTarget,
                GetElementIndex(),
                GetElementKey(),
                mapperKey.MappingTypes,
                MappingContext,
                isForDerivedTypeMapping ? this : null,
                Parent,
                createMapper: false)
            {
                MapperKey = mapperKey
            };
        }

        #region IObjectMapperKeyData Members

        object IMapperKeyData.Source => Source;

        IObjectMappingData IMapperKeyData.GetMappingData() => this;

        #endregion
    }
}