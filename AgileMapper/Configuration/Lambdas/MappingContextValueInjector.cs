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
    using ObjectPopulation;
    using static LambdaValue;
    using static Members.Member;

    internal class MappingContextValueInjector : IValueInjector
    {
        private readonly LambdaExpression _lambda;
        private readonly InvocationPosition _invocationPosition;
        private readonly ParameterExpression _contextParameter;
        private readonly Type _contextType;
        private readonly bool _isMappingContextInvokeLambda;
        private RequiredValuesSet _requiredValues;

        private MappingContextValueInjector(
            LambdaExpression lambda,
            InvocationPosition? invocationPosition,
            bool isMappingContextInvokeLambda)
            : this(lambda, invocationPosition, null, isMappingContextInvokeLambda)
        {
        }

        private MappingContextValueInjector(
            LambdaExpression lambda,
            InvocationPosition? invocationPosition,
            RequiredValuesSet requiredValues,
            bool isMappingContextInvokeLambda)
        {
            _lambda = lambda;
            _isMappingContextInvokeLambda = isMappingContextInvokeLambda;
            _requiredValues = requiredValues;
            _invocationPosition = invocationPosition.GetValueOrDefault();
            _contextParameter = lambda.Parameters[0];
            _contextType = _contextParameter.Type;
        }

        #region Factory Method

        public static IValueInjector CreateFor(
            LambdaExpression lambda,
            InvocationPosition? invocationPosition)
        {
            if (IsMappingContextInvoke(lambda))
            {
                return new MappingContextValueInjector(lambda, invocationPosition, true);
            }

            var requiredValues = GetRequiredValues(lambda);

            if (requiredValues.ValuesCount == 0)
            {
                return new NullValueInjector(lambda);
            }

            if (requiredValues.Includes(MappingContext))
            {
                return new MappingContextValueInjector(lambda, invocationPosition, requiredValues, false);
            }

            return ContextValuesValueInjector.Create(lambda, invocationPosition, requiredValues);
        }

        private static RequiredValuesSet GetRequiredValues(LambdaExpression lambda)
            => MappingContextMemberAccessFinder.GetValuesRequiredBy(lambda);

        private static bool IsMappingContextInvoke(LambdaExpression lambda)
            => lambda.Parameters.HasOne() && lambda.IsInvocation();

        #endregion

        private RequiredValuesSet GetRequiredValues()
            => _requiredValues ??= GetRequiredValues(_lambda);

        public Expression Inject(Type[] contextTypes, IMemberMapperData mapperData)
        {
            if (mapperData.MappingDataObject.Type.IsAssignableTo(_contextType))
            {
                return _lambda.ReplaceParameterWith(mapperData.MappingDataObject);
            }

            var args = new ValueInjectionArgs(_lambda, _invocationPosition, contextTypes, mapperData);
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
                replacements.Add(PropertyAccess(nameof(Parent), contextType), context.ParentAccess);
            }

            if (requiredValues.Includes(Source))
            {
                replacements.Add(PropertyAccess(RootSourceMemberName, contextType), context.SourceAccess);
            }

            if (requiredValues.Includes(Target))
            {
                replacements.Add(PropertyAccess(RootTargetMemberName, contextType), context.TargetAccess);
            }

            if (requiredValues.Includes(CreatedObject))
            {
                replacements.Add(PropertyAccess(nameof(CreatedObject), contextType), context.CreatedObject);
            }

            if (requiredValues.Includes(ElementIndex))
            {
                replacements.Add(PropertyAccess(nameof(ElementIndex), contextType), context.ElementIndex);
            }

            if (requiredValues.Includes(ElementKey))
            {
                replacements.Add(PropertyAccess(nameof(ElementKey), contextType), context.ElementKey);
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