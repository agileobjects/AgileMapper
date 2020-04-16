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
            switch (requiredValues.ValuesCount)
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

                if (requiredValues.Includes(Source))
                {
                    _value = requiredValues.Source;
                    _replacementFactory = ctx => ctx.SourceAccess;
                    return;
                }

                if (requiredValues.Includes(Target))
                {
                    _value = requiredValues.Target;
                    _replacementFactory = ctx => ctx.TargetAccess;
                    return;
                }

                if (requiredValues.Includes(ElementIndex))
                {
                    _value = requiredValues.ElementIndex;
                    _replacementFactory = ctx => ctx.ElementIndex;
                }

                if (requiredValues.Includes(CreatedObject))
                {
                    _value = requiredValues.CreatedObject;
                    _replacementFactory = ctx => ctx.CreatedObject;
                    return;
                }

                if (requiredValues.Includes(ElementKey))
                {
                    _value = requiredValues.ElementKey;
                    _replacementFactory = ctx => ctx.ElementKey;
                    return;
                }

                if (requiredValues.Includes(Parent))
                {
                    _value = requiredValues.Parent;
                    _replacementFactory = ctx => ctx.ParentAccess;
                }
            }

            public override Expression Inject(Type[] contextTypes, IMemberMapperData mapperData)
            {
                var context = CreateContext(contextTypes, mapperData);
                var replacement = _replacementFactory.Invoke(context);

                return _lambdaBody.Replace(_value, replacement, ExpressionEvaluation.Equivalator);
            }
        }

        private class MultipleContextValuesValueInjector : ContextValuesValueInjector
        {
            private readonly Expression _lambdaBody;
            private readonly bool _isInvocation;
            private readonly RequiredValuesSet _requiredValues;

            public MultipleContextValuesValueInjector(
                LambdaExpression lambda,
                InvocationPosition? invocationPosition,
                RequiredValuesSet requiredValues)
                : base(lambda, invocationPosition)
            {
                _lambdaBody = lambda.Body;
                _isInvocation = lambda.IsInvocation();
                _requiredValues = requiredValues;
            }

            private int RequiredValuesCount => _requiredValues.ValuesCount;

            public override Expression Inject(Type[] contextTypes, IMemberMapperData mapperData)
            {
                var replacements = _isInvocation
                    ? FixedSizeExpressionReplacementDictionary.WithEqualKeys(RequiredValuesCount)
                    : FixedSizeExpressionReplacementDictionary.WithEquivalentKeys(RequiredValuesCount);

                var context = CreateContext(contextTypes, mapperData);

                if (_requiredValues.Includes(MappingContext))
                {
                    replacements.Add(_requiredValues.MappingContext, context.MappingDataAccess);
                }

                if (_requiredValues.Includes(Source))
                {
                    replacements.Add(_requiredValues.Source, context.SourceAccess);
                }

                if (_requiredValues.Includes(Target))
                {
                    replacements.Add(_requiredValues.Target, context.TargetAccess);
                }

                if (_requiredValues.Includes(CreatedObject))
                {
                    replacements.Add(_requiredValues.CreatedObject, context.CreatedObject);
                }

                if (_requiredValues.Includes(ElementIndex))
                {
                    replacements.Add(_requiredValues.ElementIndex, context.ElementIndex);
                }

                if (_requiredValues.Includes(ElementKey))
                {
                    replacements.Add(_requiredValues.ElementKey, context.ElementKey);
                }

                return _lambdaBody.Replace(replacements);
            }
        }
    }
}