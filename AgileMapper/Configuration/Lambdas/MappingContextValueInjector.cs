namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching.Dictionaries;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using static LambdaValue;
    using static Members.Member;

    internal class MappingContextValueInjector : IValueInjector
    {
        private readonly LambdaExpression _lambda;
        private readonly MappingConfigInfo _configInfo;
        private readonly ParameterExpression _contextParameter;
        private readonly Type _contextType;
        private readonly bool _isMappingContextInvokeLambda;
        private RequiredValuesSet _requiredValues;

        private MappingContextValueInjector(
            LambdaExpression lambda,
            MappingConfigInfo configInfo,
            bool isMappingContextInvokeLambda)
            : this(lambda, configInfo, null, isMappingContextInvokeLambda)
        {
        }

        private MappingContextValueInjector(
            LambdaExpression lambda,
            MappingConfigInfo configInfo,
            RequiredValuesSet requiredValues,
            bool isMappingContextInvokeLambda)
        {
            _lambda = lambda;
            _isMappingContextInvokeLambda = isMappingContextInvokeLambda;
            _requiredValues = requiredValues;
            _configInfo = configInfo;
            _contextParameter = lambda.Parameters[0];
            _contextType = _contextParameter.Type;
        }

        #region Factory Method

        public static IValueInjector CreateFor(LambdaExpression lambda, MappingConfigInfo configInfo)
        {
            if (IsMappingContextInvoke(lambda))
            {
                return new MappingContextValueInjector(lambda, configInfo, true);
            }

            var requiredValues = GetRequiredValues(lambda);

            if (requiredValues.ValuesCount == 0)
            {
                return new NullValueInjector(lambda);
            }

            if (requiredValues.Includes(MappingContext))
            {
                return new MappingContextValueInjector(lambda, configInfo, requiredValues, false);
            }

            return ContextValuesValueInjector.Create(lambda, configInfo, requiredValues);
        }

        private static RequiredValuesSet GetRequiredValues(LambdaExpression lambda)
            => MappingContextMemberAccessFinder.GetValuesRequiredBy(lambda);

        private static bool IsMappingContextInvoke(LambdaExpression lambda)
            => lambda.Parameters.HasOne() && lambda.IsInvocation();

        #endregion

        public bool HasMappingContextParameter => GetRequiredValues().Includes(MappingContext);

        private RequiredValuesSet GetRequiredValues()
            => _requiredValues ??= GetRequiredValues(_lambda);

        public Expression Inject(Type[] contextTypes, IMemberMapperData mapperData)
        {
            if (mapperData.MappingDataObject.Type.IsAssignableTo(_contextType))
            {
                return _lambda.ReplaceParameterWith(mapperData.MappingDataObject);
            }

            var args = new ValueInjectionArgs(_lambda, _configInfo, contextTypes, mapperData);
            var context = args.GetAppropriateMappingContext();

            if (_isMappingContextInvokeLambda)
            {
                return args.GetInvocationContextArgument(context);
            }

            var targetContextTypes = _contextType.GetGenericTypeArguments();
            var contextType = context.IsCallback(targetContextTypes) ? _contextType : _contextType.GetAllInterfaces().First();

            var requiredValues = GetRequiredValues();

            var replacements = FixedSizeExpressionReplacementDictionary
                .WithEquivalentKeys(requiredValues.ValuesCount);

            if (requiredValues.Includes(Parent))
            {
                replacements.Add(PropertyAccess(nameof(Parent), contextType), context.GetParentAccess());
            }

            if (requiredValues.Includes(Source))
            {
                replacements.Add(PropertyAccess(RootSourceMemberName, contextType), context.GetSourceAccess());
            }

            if (requiredValues.Includes(Target))
            {
                replacements.Add(PropertyAccess(RootTargetMemberName, contextType), context.GetTargetAccess());
            }

            if (requiredValues.Includes(CreatedObject))
            {
                replacements.Add(PropertyAccess(nameof(CreatedObject), contextType), context.GetCreatedObject());
            }

            if (requiredValues.Includes(ElementIndex))
            {
                replacements.Add(PropertyAccess(nameof(ElementIndex), contextType), context.GetElementIndex());
            }

            if (requiredValues.Includes(ElementKey))
            {
                replacements.Add(PropertyAccess(nameof(ElementKey), contextType), context.GetElementKey());
            }

            return _lambda.Body.Replace(replacements);
        }

        private Expression PropertyAccess(string propertyName, Type contextType)
        {
            return Expression.Property(
                _contextParameter,
                 contextType.GetPublicInstanceProperty(propertyName));
        }
    }
}