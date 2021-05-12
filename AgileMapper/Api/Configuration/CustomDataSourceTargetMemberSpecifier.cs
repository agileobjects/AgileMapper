namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.DataSources;
    using AgileMapper.Configuration.Dictionaries;
    using AgileMapper.Configuration.Lambdas;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;
    using Members.Extensions;
    using NetStandardPolyfills;
    using Projection;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;
    using TypeConversion;
    using static System.Linq.Expressions.ExpressionType;
#if NET35
    using Expr = Microsoft.Scripting.Ast.Expression;
    using ExprType = Microsoft.Scripting.Ast.ExpressionType;
#else
    using Expr = System.Linq.Expressions.Expression;
    using ExprType = System.Linq.Expressions.ExpressionType;
#endif

    internal class CustomDataSourceTargetMemberSpecifier<TSource, TTarget> :
        ICustomDataSourceTargetMemberSpecifier<TSource, TTarget>,
        ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget>,
        IConfiguredDataSourceFactoryFactory,
        ISequencedDataSourceFactory
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly LambdaExpression _customValueLambda;
        private readonly bool _valueCouldBeSourceMember;
        private readonly ISequencedDataSourceFactory[] _sequenceDataSourceFactories;
        private ConfiguredLambdaInfo _customValueLambdaInfo;
        private ParameterInfo _targetCtorParameter;
        private LambdaExpression _targetMemberLambda;

        public CustomDataSourceTargetMemberSpecifier(
            MappingConfigInfo configInfo,
            LambdaExpression customValueLambda,
            bool valueCouldBeSourceMember)
            : this(configInfo, default)
        {
            _customValueLambda = customValueLambda;
            _valueCouldBeSourceMember = valueCouldBeSourceMember;
        }

        public CustomDataSourceTargetMemberSpecifier(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo customValueLambdaInfo)
            : this(configInfo)
        {
            _customValueLambdaInfo = customValueLambdaInfo;
        }

        private CustomDataSourceTargetMemberSpecifier(MappingConfigInfo configInfo)
        {
            _configInfo = configInfo;
            _sequenceDataSourceFactories = configInfo.GetSequenceDataSourceFactories();
        }

        private MapperContext MapperContext => _configInfo.MapperContext;

        public IConditionalMapSourceConfigurator<TSource, TTarget> Then =>
            new MappingConfigurator<TSource, TTarget>(_configInfo
                .ForSequentialConfiguration(_sequenceDataSourceFactories.Append(this)));

        public ICustomDataSourceMappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, TTargetValue>> targetMember)
        {
            ThrowIfTargetParameterSpecified(targetMember);
            ThrowIfTargetMemberFilterSpecified(targetMember);
            ThrowIfSequentialDataSourceForSimpleMember<TTargetValue>(targetMember);
            ThrowIfRedundantSourceMember<TTargetValue>(targetMember);

            return RegisterDataSourceLambda<TTargetValue>(targetMember);
        }

        IProjectionConfigContinuation<TSource, TTarget> ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget>.To<TResultValue>(
            Expression<Func<TTarget, TResultValue>> resultMember)
        {
            ThrowIfTargetParameterSpecified(resultMember);
            return RegisterDataSourceLambda<TResultValue>(resultMember);
        }

        public IMappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, Action<TTargetValue>>> targetSetMethod)
        {
            return RegisterDataSourceLambda<TTargetValue>(targetSetMethod);
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void ThrowIfTargetParameterSpecified(LambdaExpression targetMember)
        {
            if (targetMember.Body == targetMember.Parameters.FirstOrDefault())
            {
                throw new MappingConfigurationException(
                    "The target parameter is not a valid configured target; use .ToTarget() " +
                    "to map a custom data source to the target object");
            }
        }

        private void ThrowIfTargetMemberFilterSpecified<TTargetValue>(
            Expression<Func<TTarget, TTargetValue>> targetMember)
        {
            _configInfo.ThrowIfTargetMemberMatcherSpecified(
                configDescriptionFactory: ci =>
                    $"data source mapping '{GetValueLambdaInfo<TTargetValue>().GetDescription(ci)}' -> ",
                targetMember);
        }

        private void ThrowIfSequentialDataSourceForSimpleMember<TTargetValue>(
            LambdaExpression targetMemberLambda)
        {
            if (_configInfo.IsSequentialConfiguration && typeof(TTargetValue).IsSimple())
            {
                ThrowSimpleMemberSequentialDataSourceError<TTargetValue>(targetMemberLambda);
            }
        }

        private void ThrowSimpleMemberSequentialDataSourceError<TTargetValue>(
            LambdaExpression targetMemberLambda)
        {
            var targetMember = GetTargetMemberOrNull(targetMemberLambda);

            if (targetMember == null)
            {
                return;
            }

            var sourceValue = GetValueLambdaInfo<TTargetValue>();

            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "Source {0} {1} cannot be sequentially applied to target {2} {3} {4} - " +
                "simple type {2}s cannot have sequential data sources",
                sourceValue.GetDescription(_configInfo),
                GetTypeDescription(sourceValue.ReturnType),
                GetTargetMemberType(targetMember),
                targetMember.GetFriendlyTargetPath(_configInfo),
                GetTypeDescription(typeof(TTargetValue))));
        }

        private void ThrowIfRedundantSourceMember<TTargetValue>(LambdaExpression targetMemberLambda)
        {
            if (!_valueCouldBeSourceMember)
            {
                return;
            }

            var targetMember = GetTargetMemberOrNull(targetMemberLambda);

            if (targetMember == null)
            {
                return;
            }

            var valueLambdaInfo = GetValueLambdaInfo<TTargetValue>();

            ThrowIfRedundantSourceMember(valueLambdaInfo, targetMember);
        }

        private QualifiedMember GetTargetMemberOrNull(LambdaExpression targetMemberLambda)
            => targetMemberLambda.ToTargetMemberOrNull(MapperContext);

        private void ThrowIfRedundantSourceMember(ConfiguredLambdaInfo valueLambdaInfo, QualifiedMember targetMember)
        {
            if (!valueLambdaInfo.TryGetSourceMember(out var sourceMemberLambda))
            {
                return;
            }

            var mappingData = _configInfo.ToMappingData<TSource, TTarget>();

            var targetMemberMapperData = new ChildMemberMapperData(targetMember, mappingData.MapperData);
            var targetMemberMappingData = mappingData.GetChildMappingData(targetMemberMapperData);
            var bestSourceMemberMatch = SourceMemberMatcher.GetMatchFor(targetMemberMappingData);

            if (!bestSourceMemberMatch.IsUseable)
            {
                return;
            }

            var configuredSourceMember = sourceMemberLambda.ToSourceMember(MapperContext);

            if (!bestSourceMemberMatch.SourceMember.Matches(configuredSourceMember))
            {
                return;
            }

            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "Source member {0} will automatically be mapped to target {1} {2}, " +
                "and does not need to be configured",
                GetSourceMemberDescription(configuredSourceMember),
                GetTargetMemberType(targetMember),
                targetMember.GetFriendlyTargetPath(_configInfo)));
        }

        private static string GetTargetMemberType(QualifiedMember targetMember)
            => targetMember.IsConstructorParameter() ? "constructor parameter" : "member";

        private ConfiguredDataSourceFactory CreateFromLambda<TTargetValue>()
        {
            var valueLambdaInfo = GetValueLambdaInfo<TTargetValue>();

            if (IsDictionaryEntry(out var dictionaryEntryMember))
            {
                return new ConfiguredDictionaryEntryDataSourceFactory(_configInfo, valueLambdaInfo, dictionaryEntryMember);
            }

            return new ConfiguredDataSourceFactory(
                _configInfo,
                valueLambdaInfo,
#if NET35
                _targetMemberLambda.ToDlrExpression(),
#else
                _targetMemberLambda,
#endif
                _valueCouldBeSourceMember);
        }

        private ConfiguredLambdaInfo GetValueLambdaInfo<TTargetValue>() => GetValueLambdaInfo(typeof(TTargetValue));

        private ConfiguredLambdaInfo GetValueLambdaInfo(Type targetValueType)
        {
            if (_customValueLambdaInfo != null)
            {
                return _customValueLambdaInfo;
            }
#if NET35
            var customValueLambda = _customValueLambda.ToDlrExpression();
            const ExprType CONSTANT = ExprType.Constant;
#else
            var customValueLambda = _customValueLambda;
            const ExprType CONSTANT = Constant;
#endif
            if ((customValueLambda.Body.NodeType != CONSTANT) ||
                (targetValueType == typeof(object)) ||
                 customValueLambda.ReturnType.IsAssignableTo(targetValueType) ||
                _configInfo.HasTargetMemberMatcher())
            {
                return _customValueLambdaInfo = ConfiguredLambdaInfo.For(customValueLambda, _configInfo);
            }

            var convertedConstantValue = MapperContext
                .GetValueConversion(customValueLambda.Body, targetValueType);

            var funcType = Expr.GetFuncType(targetValueType);
            var valueLambda = Expr.Lambda(funcType, convertedConstantValue);
            var valueFunc = valueLambda.Compile();
            var value = valueFunc.DynamicInvoke().ToConstantExpression(targetValueType);
            var constantValueLambda = Expr.Lambda(funcType, value);
            var valueLambdaInfo = ConfiguredLambdaInfo.For(constantValueLambda, _configInfo);

            return _customValueLambdaInfo = valueLambdaInfo;
        }

        private bool IsDictionaryEntry(out DictionaryTargetMember entryMember)
        {
            if (_targetMemberLambda.Body.NodeType != Call)
            {
                entryMember = null;
                return false;
            }

            var methodCall = (MethodCallExpression)_targetMemberLambda.Body;

            if (!methodCall.Method.IsSpecialName ||
                (methodCall.Method.Name != "get_Item") ||
                !methodCall.Method.DeclaringType.IsDictionary())
            {
                // TODO: Test coverage - specified, non-dictionary indexed target member
                entryMember = null;
                return false;
            }

            var entryKeyExpression = methodCall.Arguments[0];

            if (entryKeyExpression.NodeType != Constant)
            {
                throw new MappingConfigurationException(
                    "Target dictionary entry keys must be constant string values.");
            }

            var entryKey = (string)((ConstantExpression)entryKeyExpression).Value;

            var rootMember = (DictionaryTargetMember)CreateToTargetQualifiedMember();

            entryMember = rootMember.Append(typeof(TSource), entryKey);
            return true;
        }

        private QualifiedMember CreateToTargetQualifiedMember()
        {
            return (_configInfo.TargetType == typeof(ExpandoObject))
                ? MapperContext.QualifiedMemberFactory.RootTarget<TSource, ExpandoObject>()
                : MapperContext.QualifiedMemberFactory.RootTarget<TSource, TTarget>();
        }

        public IMappingConfigContinuation<TSource, TTarget> ToCtor<TTargetParam>()
            => RegisterDataSource<TTargetParam>(cdsff => cdsff.CreateForCtorParam<TTargetParam>());

        IProjectionConfigContinuation<TSource, TTarget> ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget>.ToCtor<TTargetParam>()
            => RegisterDataSource<TTargetParam>(cdsff => cdsff.CreateForCtorParam<TTargetParam>());

        public IMappingConfigContinuation<TSource, TTarget> ToCtor(string parameterName)
            => RegisterNamedContructorParameterDataSource(parameterName);

        IProjectionConfigContinuation<TSource, TTarget> ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget>.ToCtor(
            string parameterName)
        {
            return RegisterNamedContructorParameterDataSource(parameterName);
        }

        #region Ctor Helpers

        private ConfiguredDataSourceFactory CreateForCtorParam<TParam>()
        {
            SetTargetCtorParameterForSequence(GetUniqueConstructorParameterOrThrow<TParam>());
            return CreateForCtorParam();
        }

        private MappingConfigContinuation<TSource, TTarget> RegisterNamedContructorParameterDataSource(string name)
        {
            SetTargetCtorParameterForSequence(GetUniqueConstructorParameterOrThrow<AnyParameterType>(name));

            return RegisterDataSource(_targetCtorParameter.ParameterType, cdsff => cdsff.CreateForCtorParam());
        }

        private static ParameterInfo GetUniqueConstructorParameterOrThrow<TParam>(string name = null)
        {
            var settings = new
            {
                IgnoreParameterType = typeof(TParam) == typeof(AnyParameterType),
                IgnoreParameterName = name == null
            };

            var matchingParameters = typeof(TTarget)
                .GetPublicInstanceConstructors()
                .Project(settings, (so, ctor) => new
                {
                    Ctor = ctor,
                    MatchingParameters = ctor
                        .GetParameters()
                        .FilterToArray(so, (si, p) =>
                            (si.IgnoreParameterType || (p.ParameterType == typeof(TParam))) &&
                            (si.IgnoreParameterName || (p.Name == name)))
                })
                .Filter(d => d.MatchingParameters.Any())
                .ToArray();

            if (matchingParameters.Length == 0)
            {
                throw MissingParameterException(GetParameterMatchInfo<TParam>(name, !settings.IgnoreParameterType));
            }

            var matchingParameterData = matchingParameters.First();

            if (matchingParameterData.MatchingParameters.Count > 1)
            {
                throw AmbiguousParameterException(GetParameterMatchInfo<TParam>(name, !settings.IgnoreParameterType));
            }

            var matchingParameter = matchingParameterData.MatchingParameters.First();

            return matchingParameter;
        }

        private static string GetParameterMatchInfo<TParam>(string name, bool matchParameterType)
            => matchParameterType ? GetTypeDescription(typeof(TParam)) : $"named '{name}'";

        private static Exception MissingParameterException(string parameterMatchInfo)
        {
            return new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "No constructor parameter {0} exists on type {1}",
                parameterMatchInfo,
                typeof(TTarget).GetFriendlyName()));
        }

        private static Exception AmbiguousParameterException(string parameterMatchInfo)
        {
            return new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "Multiple constructor parameters found {0} on type {1}",
                parameterMatchInfo,
                typeof(TTarget).GetFriendlyName()));
        }

        private void SetTargetCtorParameterForSequence(ParameterInfo parameter)
        {
            SetTargetCtorParameter(parameter);

            if (_sequenceDataSourceFactories == null)
            {
                return;
            }

            foreach (var dataSourceFactory in _sequenceDataSourceFactories)
            {
                dataSourceFactory.SetTargetCtorParameter(parameter);
            }
        }

        private void SetTargetCtorParameter(ParameterInfo parameter) => _targetCtorParameter = parameter;

        private ConfiguredDataSourceFactory CreateForCtorParam()
        {
            var valueLambda = GetValueLambdaInfo(_targetCtorParameter.ParameterType);
            var ctorParameterMember = Member.ConstructorParameter(_targetCtorParameter);
            var ctorParameter = CreateToTargetQualifiedMember().Append(ctorParameterMember);

            ThrowIfRedundantSourceMember(valueLambda, ctorParameter);

            return new ConfiguredDataSourceFactory(_configInfo, valueLambda, ctorParameter);
        }

        #endregion

        public IMappingConfigContinuation<TSource, TTarget> ToTarget()
            => RegisterDataSource<TTarget>(cdsff => cdsff.CreateForToTarget(isSequential: true));

        public IMappingConfigContinuation<TSource, TTarget> ToTargetInstead()
            => RegisterDataSource<TTarget>(cdsff => cdsff.CreateForToTarget(isSequential: false));

        private ConfiguredDataSourceFactoryBase CreateForToTarget(bool isSequential)
        {
            if (isSequential)
            {
                _configInfo.ForSequentialConfiguration();
            }

            var dataSourceLambda = GetValueLambdaInfo<TTarget>();
            var toTargetMember = CreateToTargetQualifiedMember();

            if (_configInfo.HasTargetMemberMatcher(out var filter))
            {
                return new ConfiguredMatcherDataSourceFactory(
                    _configInfo,
                    filter,
                    dataSourceLambda,
                    toTargetMember);
            }

            return new ConfiguredDataSourceFactory(_configInfo, dataSourceLambda, toTargetMember);
        }

        public IMappingConfigContinuation<TSource, TTarget> ToTarget<TDerivedTarget>()
            where TDerivedTarget : TTarget
        {
            var derivedTypeConfigInfo = _configInfo.Copy().ForTargetType<TDerivedTarget>();

            typeof(CustomDataSourceTargetMemberSpecifier<TSource, TTarget>)
                .GetNonPublicInstanceMethod(nameof(SetDerivedToTargetSource))
                .MakeGenericMethod(typeof(TDerivedTarget))
                .Invoke(this, new object[] { derivedTypeConfigInfo });

            return new MappingConfigurator<TSource, TTarget>(_configInfo).MapTo<TDerivedTarget>();
        }

        private void SetDerivedToTargetSource<TDerivedTarget>(MappingConfigInfo derivedTypeConfigInfo)
        {
            new MappingConfigurator<TSource, TDerivedTarget>(derivedTypeConfigInfo)
                .GetValueFactoryTargetMemberSpecifier(_customValueLambda, _customValueLambda.Type)
                .ToTarget();
        }

        private static string GetTypeDescription(Type type) => $"of type '{type.GetFriendlyName()}'";

        private MappingConfigContinuation<TSource, TTarget> RegisterDataSourceLambda<TTargetValue>(
            LambdaExpression targetMemberLambda)
        {
            SetTargetMemberForSequence(targetMemberLambda);
            return RegisterDataSource<TTargetValue>(cdsff => cdsff.CreateFromLambda<TTargetValue>());
        }

        private void SetTargetMemberForSequence(LambdaExpression targetMember)
        {
            SetTargetMember(targetMember);

            if (_sequenceDataSourceFactories == null)
            {
                return;
            }

            foreach (var dataSourceFactory in _sequenceDataSourceFactories)
            {
                dataSourceFactory.SetTargetMember(targetMember);
            }
        }

        private void SetTargetMember(LambdaExpression targetMember) => _targetMemberLambda = targetMember;

        private MappingConfigContinuation<TSource, TTarget> RegisterDataSource<TTargetValue>(
            Func<IConfiguredDataSourceFactoryFactory, ConfiguredDataSourceFactoryBase> dataSourceFactoryFactory)
        {
            return RegisterDataSource(typeof(TTargetValue), dataSourceFactoryFactory);
        }

        private MappingConfigContinuation<TSource, TTarget> RegisterDataSource(
            Type targetMemberType,
            Func<IConfiguredDataSourceFactoryFactory, ConfiguredDataSourceFactoryBase> dataSourceFactoryFactory)
        {
            ThrowIfInvalid(targetMemberType);

            if (_sequenceDataSourceFactories != null)
            {
                foreach (var dataSourceFactory in _sequenceDataSourceFactories)
                {
                    dataSourceFactory.Register(dataSourceFactoryFactory, targetMemberType);
                }

                _configInfo.SetSequenceDataSourceFactories(null);
            }

            Register(dataSourceFactoryFactory, targetMemberType);

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }

        private void Register(
            Func<IConfiguredDataSourceFactoryFactory, ConfiguredDataSourceFactoryBase> dataSourceFactoryFactory,
            Type targetMemberType)
        {
            RegisterComplexTypeFactoryMethodIfAppropriate(targetMemberType);
            MapperContext.UserConfigurations.Add(dataSourceFactoryFactory.Invoke(this));
        }

        private void ThrowIfInvalid(Type targetMemberType)
        {
            ThrowIfSimpleSourceForNonSimpleTargetMember(targetMemberType);
            ThrowIfEnumerableSourceAndTargetMismatch(targetMemberType);

            _configInfo.ThrowIfSourceTypeUnconvertible(targetMemberType);
        }

        private void ThrowIfSimpleSourceForNonSimpleTargetMember(Type targetMemberType)
        {
            if ((targetMemberType == typeof(object)) ||
                 targetMemberType.IsSimple() ||
                !ConfiguredSourceType.IsSimple() ||
                 ConversionOperatorExists(targetMemberType) ||
                _configInfo.HasTargetMemberMatcher())
            {
                return;
            }

            var sourceValue = (_customValueLambda != null)
                ? GetSourceValueDescription(_customValueLambda.Body)
                : _customValueLambdaInfo.GetDescription(_configInfo);

            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "{0} cannot be mapped to target type '{1}'",
                sourceValue,
                targetMemberType.GetFriendlyName()));
        }

        private Type ConfiguredSourceType
            => _customValueLambdaInfo?.ReturnType ?? _customValueLambda.Body.Type;

        private bool ConversionOperatorExists(Type targetMemberType)
        {
            return default(OperatorConverter).CanConvert(
                ConfiguredSourceType.GetNonNullableType(),
                targetMemberType.GetNonNullableType());
        }

        private void ThrowIfEnumerableSourceAndTargetMismatch(Type targetMemberType)
        {
            if (_customValueLambda == null)
            {
                return;
            }

            var customValue = _customValueLambda.Body;

            if ((targetMemberType.IsDictionary() || customValue.Type.IsDictionary()) ||
                (targetMemberType.IsEnumerable() == customValue.Type.IsEnumerable()))
            {
                return;
            }

            string sourceEnumerableState, targetEnumerableState;

            if (targetMemberType.IsEnumerable())
            {
                sourceEnumerableState = "Non-enumerable";
                targetEnumerableState = "enumerable";
            }
            else
            {
                sourceEnumerableState = "Enumerable";
                targetEnumerableState = "non-enumerable";
            }

            var sourceValue = GetSourceValueDescription(customValue);

            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "{0} {1} cannot be mapped to {2} target type '{3}'",
                sourceEnumerableState,
                sourceValue,
                targetEnumerableState,
                targetMemberType.GetFriendlyName()));
        }

        private string GetSourceValueDescription(Expression customValue)
        {
            if (customValue.NodeType != MemberAccess)
            {
                return $"Source value {customValue.ToReadableString()} {GetTypeDescription(customValue.Type)}";
            }

            var sourceMember = customValue.ToSourceMember(MapperContext);

            return GetSourceMemberDescription(sourceMember);
        }

        private string GetSourceMemberDescription(IQualifiedMember sourceMember)
        {
            var rootSourceMember = MapperContext.QualifiedMemberFactory.RootSource<TSource, TTarget>();
            var sourceMemberPath = sourceMember.GetFriendlyMemberPath(rootSourceMember);

            return sourceMemberPath + " " + GetTypeDescription(sourceMember.Type);
        }

        private void RegisterComplexTypeFactoryMethodIfAppropriate(Type targetMemberType)
        {
            if (SourceIsComplexTypeFactoryMethod(targetMemberType))
            {
                typeof(CustomDataSourceTargetMemberSpecifier<TSource, TTarget>)
                    .GetNonPublicInstanceMethod(nameof(RegisterComplexTypeFactoryMethod))
                    .MakeGenericMethod(_customValueLambda.Type)
                    .Invoke(this, Enumerable<object>.EmptyArray);
            }
        }

        private bool SourceIsComplexTypeFactoryMethod(Type targetMemberType)
        {
            if ((_customValueLambda?.Body.NodeType != Call) || !targetMemberType.IsComplex())
            {
                return false;
            }

            var methodCall = (MethodCallExpression)_customValueLambda.Body;

            if (methodCall.Method.IsStatic)
            {
                return true;
            }

            var rootExpression = methodCall
#if NET35
                .ToDlrExpression()
#endif
                .GetRootExpression();

            return rootExpression.NodeType != ExprType.Parameter;
        }

        private void RegisterComplexTypeFactoryMethod<TSourceValue>()
        {
            new FactorySpecifier<TSource, TTarget, TSourceValue>(_configInfo)
                .Using(_customValueLambda);
        }

        private struct AnyParameterType { }

        #region IConfiguredDataSourceFactoryFactory Members

        ConfiguredDataSourceFactory IConfiguredDataSourceFactoryFactory.CreateForCtorParam()
            => CreateForCtorParam();

        ConfiguredDataSourceFactory IConfiguredDataSourceFactoryFactory.CreateForCtorParam<TTargetValue>()
            => CreateForCtorParam<TTargetValue>();

        ConfiguredDataSourceFactory IConfiguredDataSourceFactoryFactory.CreateFromLambda<TTargetValue>()
            => CreateFromLambda<TTargetValue>();

        ConfiguredDataSourceFactoryBase IConfiguredDataSourceFactoryFactory.CreateForToTarget(bool isSequential)
            => CreateForToTarget(isSequential);

        #endregion

        #region ISequencedDataSourceFactory Members

        void ISequencedDataSourceFactory.SetTargetCtorParameter(ParameterInfo parameter)
            => SetTargetCtorParameter(parameter);

        void ISequencedDataSourceFactory.SetTargetMember(LambdaExpression targetMember)
            => SetTargetMember(targetMember);

        void ISequencedDataSourceFactory.Register(
            Func<IConfiguredDataSourceFactoryFactory, ConfiguredDataSourceFactoryBase> dataSourceFactoryFactory,
            Type targetMemberType)
        {
            Register(dataSourceFactoryFactory, targetMemberType);
        }

        #endregion
    }
}