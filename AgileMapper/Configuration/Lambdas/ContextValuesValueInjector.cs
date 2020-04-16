namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching;
    using Caching.Dictionaries;
    using Extensions.Internal;
    using Members;
    using ObjectPopulation;
    using static LambdaValue;

    internal delegate Expression ContextValueFactory(ValueInjectionContext context);

    internal abstract class ContextValuesValueInjector : IValueInjector
    {
        private readonly LambdaExpression _lambda;
        private readonly InvocationPosition _invocationPosition;

        protected ContextValuesValueInjector(
            LambdaExpression lambda,
            InvocationPosition? invocationPosition)
        {
            _lambda = lambda;
            _invocationPosition = invocationPosition.GetValueOrDefault();
        }

        public static IValueInjector Create(
            LambdaExpression lambda,
            InvocationPosition? invocationPosition)
        {
            var requiredValues = ParametersAccessFinder.GetValuesRequiredBy(lambda);

            return Create(lambda, invocationPosition, requiredValues);
        }

        public static IValueInjector Create(
            LambdaExpression lambda,
            InvocationPosition? invocationPosition,
            RequiredValuesSet requiredValues)
        {
            switch (requiredValues.Values.Count())
            {
                case 0:
                    return new NullValueInjector(lambda);

                case 1:
                    return new SingleContextValueValueInjector(lambda, invocationPosition, requiredValues);

                default:
                    return new MultipleContextValuesValueInjector(lambda, invocationPosition, requiredValues);
            }
        }

        public abstract Expression Inject(Type[] contextTypes, IMemberMapperData mapperData);

        protected ValueInjectionContext CreateContext(Type[] contextTypes, IMemberMapperData mapperData)
        {
            var args = new ValueInjectionArgs(_lambda, _invocationPosition, contextTypes, mapperData);
            var context = args.GetAppropriateMappingContext();

            return context;
        }

        private class SingleContextValueValueInjector : ContextValuesValueInjector
        {
            private readonly Expression _lambdaBody;
            private readonly Expression _value;
            private readonly ContextValueFactory _replacementFactory;

            public SingleContextValueValueInjector(
                LambdaExpression lambda,
                InvocationPosition? invocationPosition,
                RequiredValuesSet requiredValues)
                : base(lambda, invocationPosition)
            {
                _lambdaBody = lambda.Body;

                var requiredLambdaValues = requiredValues.Values;

                if (requiredLambdaValues.Has(Source))
                {
                    _value = requiredValues.Source;
                    _replacementFactory = ctx => ctx.SourceAccess;
                    return;
                }

                if (requiredLambdaValues.Has(Target))
                {
                    _value = requiredValues.Target;
                    _replacementFactory = ctx => ctx.TargetAccess;
                    return;
                }

                if (requiredLambdaValues.Has(CreatedObject))
                {
                    _value = requiredValues.CreatedObject;
                    _replacementFactory = ctx => ctx.CreatedObject;
                    return;
                }

                if (requiredLambdaValues.Has(ElementIndex))
                {
                    _value = requiredValues.ElementIndex;
                    _replacementFactory = ctx => ctx.ElementIndex;
                }
            }

            public override Expression Inject(Type[] contextTypes, IMemberMapperData mapperData)
            {
                var context = CreateContext(contextTypes, mapperData);
                var replacement = _replacementFactory.Invoke(context);

                return _lambdaBody.Replace(_value, replacement);
            }
        }

        private class MultipleContextValuesValueInjector : ContextValuesValueInjector
        {
            private readonly Expression _lambdaBody;
            private readonly ISimpleDictionary<Expression, ContextValueFactory> _replacements;

            public MultipleContextValuesValueInjector(
                LambdaExpression lambda,
                InvocationPosition? invocationPosition,
                RequiredValuesSet requiredValues)
                : base(lambda, invocationPosition)
            {
                _lambdaBody = lambda.Body;

                var requiredLambdaValues = requiredValues.Values;

                _replacements = new FixedSizeSimpleDictionary<Expression, ContextValueFactory>(
                    requiredLambdaValues.Count(),
                    ReferenceEqualsComparer<Expression>.Default);

                if (requiredLambdaValues.Has(MappingContext))
                {
                    _replacements.Add(requiredValues.MappingContext, ctx => ctx.MappingDataAccess);
                }

                if (requiredLambdaValues.Has(Source))
                {
                    _replacements.Add(requiredValues.Source, ctx => ctx.SourceAccess);
                }

                if (requiredLambdaValues.Has(Target))
                {
                    _replacements.Add(requiredValues.Target, ctx => ctx.TargetAccess);
                }

                if (requiredLambdaValues.Has(CreatedObject))
                {
                    _replacements.Add(requiredValues.CreatedObject, ctx => ctx.CreatedObject);
                }

                if (requiredLambdaValues.Has(ElementIndex))
                {
                    _replacements.Add(requiredValues.ElementIndex, ctx => ctx.ElementIndex);
                }

                if (requiredLambdaValues.Has(ElementKey))
                {
                    _replacements.Add(requiredValues.ElementKey, ctx => ctx.ElementKey);
                }
            }

            public override Expression Inject(Type[] contextTypes, IMemberMapperData mapperData)
            {
                var context = CreateContext(contextTypes, mapperData);

                var replacements = FixedSizeExpressionReplacementDictionary
                    .WithEqualKeys(_replacements.Count);

                foreach (var valueFactoryByValue in _replacements)
                {
                    var value = valueFactoryByValue.Key;
                    var replacement = valueFactoryByValue.Value.Invoke(context);

                    replacements.Add(value, replacement);
                }

                return _lambdaBody.Replace(replacements);
            }
        }
    }
}