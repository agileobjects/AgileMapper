namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AgileMapper.Configuration;
    using AgileMapper.Configuration.Dictionaries;
    using AgileMapper.Configuration.Lambdas;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;
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
        ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly LambdaExpression _customValueLambda;
        private readonly bool _valueCouldBeSourceMember;
        private ConfiguredLambdaInfo _customValueLambdaInfo;

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
            ConfiguredLambdaInfo customValueLambda)
        {
            _configInfo = configInfo;
            _customValueLambdaInfo = customValueLambda;
        }

        private MapperContext MapperContext => _configInfo.MapperContext;

        public ICustomDataSourceMappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, TTargetValue>> targetMember)
        {
            ThrowIfTargetParameterSpecified(targetMember);
            ThrowIfSequentialDataSourceForSimpleMember<TTargetValue>(targetMember);
            ThrowIfRedundantSourceMember<TTargetValue>(targetMember);

            return RegisterDataSource<TTargetValue>(() => CreateFromLambda<TTargetValue>(targetMember));
        }

        IProjectionConfigContinuation<TSource, TTarget> ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget>.To<TResultValue>(
            Expression<Func<TTarget, TResultValue>> resultMember)
        {
            ThrowIfTargetParameterSpecified(resultMember);

            return RegisterDataSource<TResultValue>(() => CreateFromLambda<TResultValue>(resultMember));
        }

        public IMappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, Action<TTargetValue>>> targetSetMethod)
        {
            return RegisterDataSource<TTargetValue>(() => CreateFromLambda<TTargetValue>(targetSetMethod));
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
            => targetMemberLambda.ToTargetMember(MapperContext, nt => { });

        private void ThrowIfRedundantSourceMember(ConfiguredLambdaInfo valueLambdaInfo, QualifiedMember targetMember)
        {
            if (!valueLambdaInfo.IsSourceMember(out var sourceMemberLambda))
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

        private ConfiguredDataSourceFactory CreateFromLambda<TTargetValue>(LambdaExpression targetMemberLambda)
        {
            var valueLambdaInfo = GetValueLambdaInfo<TTargetValue>();

            if (IsDictionaryEntry(targetMemberLambda, out var dictionaryEntryMember))
            {
                return new ConfiguredDictionaryEntryDataSourceFactory(_configInfo, valueLambdaInfo, dictionaryEntryMember);
            }

            return CreateDataSourceFactory(valueLambdaInfo, targetMemberLambda);
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
                 customValueLambda.ReturnType.IsAssignableTo(targetValueType))
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

        private bool IsDictionaryEntry(LambdaExpression targetMemberLambda, out DictionaryTargetMember entryMember)
        {
            if (targetMemberLambda.Body.NodeType != Call)
            {
                entryMember = null;
                return false;
            }

            var methodCall = (MethodCallExpression)targetMemberLambda.Body;

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

            var rootMember = (DictionaryTargetMember)CreateRootTargetQualifiedMember();

            entryMember = rootMember.Append(typeof(TSource), entryKey);
            return true;
        }

        private QualifiedMember CreateRootTargetQualifiedMember()
        {
            return (_configInfo.TargetType == typeof(ExpandoObject))
                ? MapperContext.QualifiedMemberFactory.RootTarget<TSource, ExpandoObject>()
                : MapperContext.QualifiedMemberFactory.RootTarget<TSource, TTarget>();
        }

        private ConfiguredDataSourceFactory CreateDataSourceFactory(
            ConfiguredLambdaInfo valueLambdaInfo,
            LambdaExpression targetMemberLambda)
        {
            return new ConfiguredDataSourceFactory(
                _configInfo,
                valueLambdaInfo,
#if NET35
                targetMemberLambda.ToDlrExpression(),
#else
                targetMemberLambda,
#endif
                _valueCouldBeSourceMember);
        }

        public IMappingConfigContinuation<TSource, TTarget> ToCtor<TTargetParam>()
            => RegisterDataSource<TTargetParam>(CreateForCtorParam<TTargetParam>);

        IProjectionConfigContinuation<TSource, TTarget> ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget>.ToCtor<TTargetParam>()
            => RegisterDataSource<TTargetParam>(CreateForCtorParam<TTargetParam>);

        public IMappingConfigContinuation<TSource, TTarget> ToCtor(string parameterName)
            => RegisterNamedContructorParameterDataSource(parameterName);

        IProjectionConfigContinuation<TSource, TTarget> ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget>.ToCtor(
            string parameterName)
        {
            return RegisterNamedContructorParameterDataSource(parameterName);
        }

        #region Ctor Helpers

        private ConfiguredDataSourceFactory CreateForCtorParam<TParam>()
            => CreateForCtorParam(GetUniqueConstructorParameterOrThrow<TParam>());

        private MappingConfigContinuation<TSource, TTarget> RegisterNamedContructorParameterDataSource(string name)
        {
            var parameter = GetUniqueConstructorParameterOrThrow<AnyParameterType>(name);

            return RegisterDataSource(parameter.ParameterType, () => CreateForCtorParam(parameter));
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

        private ConfiguredDataSourceFactory CreateForCtorParam(ParameterInfo parameter)
        {
            var valueLambda = GetValueLambdaInfo(parameter.ParameterType);
            var constructorParameter = CreateRootTargetQualifiedMember().Append(Member.ConstructorParameter(parameter));

            ThrowIfRedundantSourceMember(valueLambda, constructorParameter);

            return new ConfiguredDataSourceFactory(_configInfo, valueLambda, constructorParameter);
        }

        #endregion

        public IMappingConfigContinuation<TSource, TTarget> ToTarget()
        {
            return RegisterDataSource<TTarget>(() => new ConfiguredDataSourceFactory(
                _configInfo,
                GetValueLambdaInfo<TTarget>(),
                CreateRootTargetQualifiedMember()));
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

        // ReSharper disable once UnusedMember.Local
        private void SetDerivedToTargetSource<TDerivedTarget>(MappingConfigInfo derivedTypeConfigInfo)
        {
            new MappingConfigurator<TSource, TDerivedTarget>(derivedTypeConfigInfo)
                .GetValueFactoryTargetMemberSpecifier(_customValueLambda, _customValueLambda.Type)
                .ToTarget();
        }

        private static string GetTypeDescription(Type type) => $"of type '{type.GetFriendlyName()}'";

        private MappingConfigContinuation<TSource, TTarget> RegisterDataSource<TTargetValue>(
            Func<ConfiguredDataSourceFactory> dataSourceFactoryFactory)
        {
            return RegisterDataSource(typeof(TTargetValue), dataSourceFactoryFactory);
        }

        private MappingConfigContinuation<TSource, TTarget> RegisterDataSource(
            Type targetMemberType,
            Func<ConfiguredDataSourceFactory> dataSourceFactoryFactory)
        {
            ThrowIfInvalid(targetMemberType);

            RegisterComplexTypeFactoryMethodIfAppropriate(targetMemberType);
            MapperContext.UserConfigurations.Add(dataSourceFactoryFactory.Invoke());

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
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
               !_customValueLambda.Body.Type.IsSimple() ||
                 ConversionOperatorExists(targetMemberType))
            {
                return;
            }

            var sourceValue = GetSourceValueDescription(_customValueLambda.Body);

            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "{0} cannot be mapped to target type '{1}'",
                sourceValue,
                targetMemberType.GetFriendlyName()));
        }

        private bool ConversionOperatorExists(Type targetMemberType)
        {
            return default(OperatorConverter).CanConvert(
                _customValueLambda.Body.Type.GetNonNullableType(),
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
    }
}