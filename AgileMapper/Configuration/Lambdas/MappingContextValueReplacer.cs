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

    internal class MappingContextValueReplacer : IValueReplacer
    {
        private readonly LambdaExpression _lambda;
        private readonly MappingConfigInfo _configInfo;
        private readonly ParameterExpression _contextParameter;
        private readonly Type _contextType;
        private readonly bool _isMappingDataParameterLambda;
        private RequiredValuesSet _requiredValues;

        private MappingContextValueReplacer(
            LambdaExpression lambda,
            MappingConfigInfo configInfo,
            bool isMappingDataParameterLambda)
            : this(lambda, configInfo, null, isMappingDataParameterLambda)
        {
        }

        private MappingContextValueReplacer(
            LambdaExpression lambda,
            MappingConfigInfo configInfo,
            RequiredValuesSet requiredValues,
            bool isMappingDataParameterLambda)
        {
            _lambda = lambda;
            _isMappingDataParameterLambda = isMappingDataParameterLambda;
            _requiredValues = requiredValues;
            _configInfo = configInfo;
            _contextParameter = lambda.Parameters[0];
            _contextType = _contextParameter.Type;
        }

        #region Factory Method

        public static IValueReplacer CreateFor(LambdaExpression lambda, MappingConfigInfo configInfo)
        {
            if (IsMappingDataParameterFuncInvoke(lambda))
            {
                return new MappingContextValueReplacer(lambda, configInfo, true);
            }

            var requiredValues = GetRequiredValues(lambda);

            if (requiredValues.ValuesCount == 0)
            {
                return new NullValueReplacer(lambda);
            }

            if (requiredValues.Includes(MappingContext))
            {
                return new MappingContextValueReplacer(lambda, configInfo, requiredValues, false);
            }

            return ContextValuesValueReplacer.Create(lambda, configInfo, requiredValues);
        }

        private static bool IsMappingDataParameterFuncInvoke(LambdaExpression lambda)
            => lambda.Parameters.HasOne() && lambda.IsInvocation();

        private static RequiredValuesSet GetRequiredValues(LambdaExpression lambda)
            => MappingContextMemberAccessFinder.GetValuesRequiredBy(lambda);

        #endregion

        public bool NeedsMappingData => GetRequiredValues().Includes(MappingContext);

        private RequiredValuesSet GetRequiredValues()
            => _requiredValues ??= GetRequiredValues(_lambda);

        public Expression Replace(Type[] contextTypes, IMemberMapperData mapperData)
        {
            if (_isMappingDataParameterLambda && mapperData.TypesMatch(contextTypes))
            {
                return _lambda.ReplaceParameterWith(mapperData
                    .GetToMappingDataCall(contextTypes));
            }

            var args = new ValueReplacementArgs(_lambda, _configInfo, contextTypes, mapperData);
            var context = args.GetValueReplacementContext();

            if (_isMappingDataParameterLambda)
            {
                return args.GetFuncInvokeMappingDataArgument(context);
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