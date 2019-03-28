namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using AgileMapper.Configuration;
    using DataSources;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;
    using NetStandardPolyfills;
    using Projection;
    using ReadableExpressions.Extensions;
#if NET35
    using Dlr = Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.Expression;
#else
    using static System.Linq.Expressions.Expression;
#endif

    internal class CustomDataSourceTargetMemberSpecifier<TSource, TTarget> :
        ICustomMappingDataSourceTargetMemberSpecifier<TSource, TTarget>,
        ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget>
    {
        private readonly MappingConfigInfo _configInfo;
        private readonly LambdaExpression _customValueLambda;
        private readonly bool _valueCouldBeSourceMember;
        private readonly ConfiguredLambdaInfo _customValueLambdaInfo;

        public CustomDataSourceTargetMemberSpecifier(
            MappingConfigInfo configInfo,
            LambdaExpression customValueLambda,
            bool valueCouldBeSourceMember)
            : this(configInfo, default(ConfiguredLambdaInfo))
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

        public ICustomDataSourceMappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, TTargetValue>> targetMember)
        {
            ThrowIfTargetParameterSpecified(targetMember);
            ThrowIfSimpleSourceForNonSimpleTargetMember(typeof(TTargetValue));

            return RegisterDataSource<TTargetValue>(() => CreateFromLambda<TTargetValue>(targetMember));
        }

        IProjectionConfigContinuation<TSource, TTarget> ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget>.To<TResultValue>(
            Expression<Func<TTarget, TResultValue>> resultMember)
        {
            ThrowIfTargetParameterSpecified(resultMember);
            ThrowIfSimpleSourceForNonSimpleTargetMember(typeof(TResultValue));

            return RegisterDataSource<TResultValue>(() => CreateFromLambda<TResultValue>(resultMember));
        }

        public IMappingConfigContinuation<TSource, TTarget> To<TTargetValue>(
            Expression<Func<TTarget, Action<TTargetValue>>> targetSetMethod)
        {
            ThrowIfSimpleSourceForNonSimpleTargetMember(typeof(TTargetValue));

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

        private void ThrowIfSimpleSourceForNonSimpleTargetMember(Type targetMemberType)
        {
            if ((targetMemberType != typeof(object)) && !targetMemberType.IsSimple())
            {
                ThrowIfSimpleSource(targetMemberType);
            }
        }

        private ConfiguredDataSourceFactory CreateFromLambda<TTargetValue>(LambdaExpression targetMemberLambda)
        {
            var valueLambdaInfo = GetValueLambdaInfo(typeof(TTargetValue));

            if (IsDictionaryEntry(targetMemberLambda, out var dictionaryEntryMember))
            {
                return new ConfiguredDictionaryEntryDataSourceFactory(_configInfo, valueLambdaInfo, dictionaryEntryMember);
            }

            return CreateDataSourceFactory(valueLambdaInfo, targetMemberLambda);
        }

        private ConfiguredLambdaInfo GetValueLambdaInfo(Type targetValueType)
        {
            if (_customValueLambdaInfo != null)
            {
                return _customValueLambdaInfo;
            }

#if NET35
            var customValueLambda = _customValueLambda.ToDlrExpression();
            const Dlr.ExpressionType CONSTANT = Dlr.ExpressionType.Constant;
#else
            var customValueLambda = _customValueLambda;
            const ExpressionType CONSTANT = ExpressionType.Constant;
#endif
            if ((customValueLambda.Body.NodeType != CONSTANT) ||
                (targetValueType == typeof(object)) ||
                 customValueLambda.ReturnType.IsAssignableTo(targetValueType))
            {
                return ConfiguredLambdaInfo.For(customValueLambda);
            }

            var convertedConstantValue = _configInfo
                .MapperContext
                .ValueConverters
                .GetConversion(customValueLambda.Body, targetValueType);

            var funcType = GetFuncType(targetValueType);
            var valueLambda = Lambda(funcType, convertedConstantValue);
            var valueFunc = valueLambda.Compile();
            var value = valueFunc.DynamicInvoke().ToConstantExpression(targetValueType);
            var constantValueLambda = Lambda(funcType, value);
            var valueLambdaInfo = ConfiguredLambdaInfo.For(constantValueLambda);

            return valueLambdaInfo;
        }

        private bool IsDictionaryEntry(LambdaExpression targetMemberLambda, out DictionaryTargetMember entryMember)
        {
            if (targetMemberLambda.Body.NodeType != ExpressionType.Call)
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

            if (entryKeyExpression.NodeType != ExpressionType.Constant)
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
                ? _configInfo.MapperContext.QualifiedMemberFactory.RootTarget<TSource, ExpandoObject>()
                : _configInfo.MapperContext.QualifiedMemberFactory.RootTarget<TSource, TTarget>();
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
            => RegisterDataSource<object>(() => CreateForCtorParam(parameterName));

        IProjectionConfigContinuation<TSource, TTarget> ICustomProjectionDataSourceTargetMemberSpecifier<TSource, TTarget>.ToCtor(
            string parameterName)
        {
            return RegisterDataSource<object>(() => CreateForCtorParam(parameterName));
        }

        #region Ctor Helpers

        private ConfiguredDataSourceFactory CreateForCtorParam<TParam>()
            => CreateForCtorParam<TParam>(GetUniqueConstructorParameterOrThrow<TParam>());

        private ConfiguredDataSourceFactory CreateForCtorParam(string name)
            => CreateForCtorParam<object>(GetUniqueConstructorParameterOrThrow<AnyParameterType>(name));

        private static ParameterInfo GetUniqueConstructorParameterOrThrow<TParam>(string name = null)
        {
            var ignoreParameterType = typeof(TParam) == typeof(AnyParameterType);
            var ignoreParameterName = name == null;

            var matchingParameters = typeof(TTarget)
                .GetPublicInstanceConstructors()
                .Project(ctor => new
                {
                    Ctor = ctor,
                    MatchingParameters = ctor
                        .GetParameters()
                        .Filter(p =>
                            (ignoreParameterType || (p.ParameterType == typeof(TParam))) &&
                            (ignoreParameterName || (p.Name == name)))
                        .ToArray()
                })
                .Filter(d => d.MatchingParameters.Any())
                .ToArray();

            if (matchingParameters.Length == 0)
            {
                throw MissingParameterException(GetParameterMatchInfo<TParam>(name, !ignoreParameterType));
            }

            var matchingParameterData = matchingParameters.First();

            if (matchingParameterData.MatchingParameters.Length > 1)
            {
                throw AmbiguousParameterException(GetParameterMatchInfo<TParam>(name, !ignoreParameterType));
            }

            var matchingParameter = matchingParameterData.MatchingParameters.First();

            return matchingParameter;
        }

        private static string GetParameterMatchInfo<TParam>(string name, bool matchParameterType)
            => matchParameterType ? "of type " + typeof(TParam).GetFriendlyName() : "named '" + name + "'";

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

        private ConfiguredDataSourceFactory CreateForCtorParam<TParam>(ParameterInfo parameter)
        {
            var valueLambda = GetValueLambdaInfo(typeof(TParam));
            var constructorParameter = CreateRootTargetQualifiedMember().Append(Member.ConstructorParameter(parameter));

            return new ConfiguredDataSourceFactory(_configInfo, valueLambda, constructorParameter);
        }

        #endregion

        public IMappingConfigContinuation<TSource, TTarget> ToTarget()
        {
            ThrowIfSimpleSourceForNonSimpleTargetMember(typeof(TTarget));
            ThrowIfEnumerableSourceAndTargetMismatch(typeof(TTarget));

            return RegisterDataSource<TTarget>(() => new ConfiguredDataSourceFactory(
                _configInfo,
                GetValueLambdaInfo(typeof(TTarget)),
                CreateRootTargetQualifiedMember()));
        }

        public IMappingConfigContinuation<TSource, TDerivedTarget> ToTarget<TDerivedTarget>()
            where TDerivedTarget : TTarget
        {
            new MappingConfigurator<TSource, TTarget>(_configInfo).MapTo<TDerivedTarget>();

            var derivedTypeConfigInfo = _configInfo.Copy().ForTargetType<TDerivedTarget>();

            typeof(CustomDataSourceTargetMemberSpecifier<TSource, TTarget>)
                .GetNonPublicInstanceMethod(nameof(SetDerivedToTargetSource))
                .MakeGenericMethod(typeof(TDerivedTarget))
                .Invoke(this, new object[] { derivedTypeConfigInfo });

            return new MappingConfigContinuation<TSource, TDerivedTarget>(derivedTypeConfigInfo);
        }

        // ReSharper disable once UnusedMember.Local
        private void SetDerivedToTargetSource<TDerivedTarget>(MappingConfigInfo derivedTypeConfigInfo)
        {
            new MappingConfigurator<TSource, TDerivedTarget>(derivedTypeConfigInfo)
                .GetValueFactoryTargetMemberSpecifier(_customValueLambda, _customValueLambda.Type)
                .ToTarget();
        }

        private void ThrowIfSimpleSource(Type targetMemberType)
        {
            var customValue = _customValueLambda.Body;

            if (!customValue.Type.IsSimple())
            {
                return;
            }

            var sourceValue = GetSourceValue(customValue);

            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "{0}'{1}' cannot be mapped to target type '{2}'",
                sourceValue,
                customValue.Type.GetFriendlyName(),
                targetMemberType.GetFriendlyName()));
        }

        private void ThrowIfEnumerableSourceAndTargetMismatch(Type targetMemberType)
        {
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

            var sourceValue = GetSourceValue(customValue);

            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "{0} {1}'{2}' cannot be mapped to {3} target type '{4}'",
                sourceEnumerableState,
                sourceValue,
                customValue.Type.GetFriendlyName(),
                targetEnumerableState,
                targetMemberType.GetFriendlyName()));
        }

        private string GetSourceValue(Expression customValue)
        {
            if (customValue.NodeType != ExpressionType.MemberAccess)
            {
                return "Source type ";
            }

            var rootSourceMember = _configInfo.MapperContext.QualifiedMemberFactory.RootSource<TSource, TTarget>();
            var sourceMember = customValue.ToSourceMember(_configInfo.MapperContext);
            var sourceValue = sourceMember.GetFriendlyMemberPath(rootSourceMember) + " of type ";

            return sourceValue;
        }

        private MappingConfigContinuation<TSource, TTarget> RegisterDataSource<TTargetValue>(
            Func<ConfiguredDataSourceFactory> factoryFactory)
        {
            _configInfo.ThrowIfSourceTypeUnconvertible<TTargetValue>();
            _configInfo.MapperContext.UserConfigurations.Add(factoryFactory.Invoke());

            return new MappingConfigContinuation<TSource, TTarget>(_configInfo);
        }

        private struct AnyParameterType { }
    }
}