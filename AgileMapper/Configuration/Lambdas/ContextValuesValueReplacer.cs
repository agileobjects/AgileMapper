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
    using static LambdaValue;

    internal delegate Expression ContextValueFactory(ValueReplacementContext context);

    internal abstract class ContextValuesValueReplacer : IValueReplacer
    {
        private readonly LambdaExpression _lambda;
        private readonly MappingConfigInfo _configInfo;

        protected ContextValuesValueReplacer(LambdaExpression lambda, MappingConfigInfo configInfo)
        {
            _lambda = lambda;
            _configInfo = configInfo;
        }

        public static IValueReplacer Create(LambdaExpression lambda, MappingConfigInfo configInfo)
        {
            var requiredValues = ParametersAccessFinder.GetValuesRequiredBy(lambda);

            return Create(lambda, configInfo, requiredValues);
        }

        public static IValueReplacer Create(
            LambdaExpression lambda,
            MappingConfigInfo configInfo,
            RequiredValuesSet requiredValues)
        {
            switch (requiredValues.ValuesCount)
            {
                case 0:
                    return new NullValueReplacer(lambda);

                case 1:
                    return new SingleContextValueValueReplacer(lambda, configInfo, requiredValues);

                default:
                    return new MultipleContextValuesValueReplacer(lambda, configInfo, requiredValues);
            }
        }

        public abstract bool NeedsMappingData { get; }

        public abstract Expression Replace(Type[] contextTypes, IMemberMapperData mapperData);

        protected ValueReplacementContext CreateContext(Type[] contextTypes, IMemberMapperData mapperData)
        {
            var args = new ValueReplacementArgs(_lambda, _configInfo, contextTypes, mapperData);
            var context = args.GetValueReplacementContext();

            return context;
        }

        private class SingleContextValueValueReplacer : ContextValuesValueReplacer
        {
            private readonly Expression _lambdaBody;
            private readonly Expression _value;
            private readonly ContextValueFactory _replacementFactory;

            public SingleContextValueValueReplacer(
                LambdaExpression lambda,
                MappingConfigInfo configInfo,
                RequiredValuesSet requiredValues)
                : base(lambda, configInfo)
            {
                _lambdaBody = lambda.Body;

                if (requiredValues.Includes(Source))
                {
                    _value = requiredValues.Source;
                    _replacementFactory = ctx => ctx.GetSourceAccess();
                    return;
                }

                if (requiredValues.Includes(Target))
                {
                    _value = requiredValues.Target;
                    _replacementFactory = ctx => ctx.GetTargetAccess();
                    return;
                }

                if (requiredValues.Includes(ElementIndex))
                {
                    _value = requiredValues.ElementIndex;

                    _replacementFactory =
                        ctx => ctx.GetElementIndex().GetConversionTo<int?>();
                }

                if (requiredValues.Includes(CreatedObject))
                {
                    _value = requiredValues.CreatedObject;
                    _replacementFactory = ctx => ctx.GetCreatedObject();
                    return;
                }

                if (requiredValues.Includes(ElementKey))
                {
                    _value = requiredValues.ElementKey;
                    _replacementFactory = ctx => ctx.GetElementKey();
                    return;
                }

                if (requiredValues.Includes(Parent))
                {
                    _value = requiredValues.Parent;
                    _replacementFactory = ctx => ctx.GetParentAccess();
                }
            }

            public override bool NeedsMappingData => false;

            public override Expression Replace(Type[] contextTypes, IMemberMapperData mapperData)
            {
                var context = CreateContext(contextTypes, mapperData);
                var replacement = _replacementFactory.Invoke(context);

                return _lambdaBody.Replace(_value, replacement, ExpressionEvaluation.Equivalator);
            }
        }

        private class MultipleContextValuesValueReplacer : ContextValuesValueReplacer
        {
            private readonly Expression _lambdaBody;
            private readonly bool _isInvocation;
            private readonly RequiredValuesSet _requiredValues;

            public MultipleContextValuesValueReplacer(
                LambdaExpression lambda,
                MappingConfigInfo configInfo,
                RequiredValuesSet requiredValues)
                : base(lambda, configInfo)
            {
                _lambdaBody = lambda.Body;
                _isInvocation = lambda.IsInvocation();
                _requiredValues = requiredValues;
            }

            private int RequiredValuesCount => _requiredValues.ValuesCount;

            public override bool NeedsMappingData => _requiredValues.Includes(MappingContext);

            public override Expression Replace(Type[] contextTypes, IMemberMapperData mapperData)
            {
                var replacements = _isInvocation
                    ? FixedSizeExpressionReplacementDictionary.WithEqualKeys(RequiredValuesCount)
                    : FixedSizeExpressionReplacementDictionary.WithEquivalentKeys(RequiredValuesCount);

                var context = CreateContext(contextTypes, mapperData);

                if (_requiredValues.Includes(MappingContext))
                {
                    replacements.Add(_requiredValues.MappingContext, context.GetToMappingDataCall());
                }

                if (_requiredValues.Includes(Source))
                {
                    replacements.Add(_requiredValues.Source, context.GetSourceAccess());
                }

                if (_requiredValues.Includes(Target))
                {
                    replacements.Add(_requiredValues.Target, context.GetTargetAccess());
                }

                if (_requiredValues.Includes(CreatedObject))
                {
                    replacements.Add(_requiredValues.CreatedObject, context.GetCreatedObject());
                }

                if (_requiredValues.Includes(ElementIndex))
                {
                    replacements.Add(
                        _requiredValues.ElementIndex,
                        context.GetElementIndex().GetConversionTo<int?>());
                }

                if (_requiredValues.Includes(ElementKey))
                {
                    replacements.Add(_requiredValues.ElementKey, context.GetElementKey());
                }

                return _lambdaBody.Replace(replacements);
            }
        }
    }
}