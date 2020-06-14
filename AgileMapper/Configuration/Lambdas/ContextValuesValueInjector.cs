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

    internal delegate Expression ContextValueFactory(ValueInjectionContext context);

    internal abstract class ContextValuesValueInjector : IValueInjector
    {
        private readonly LambdaExpression _lambda;
        private readonly MappingConfigInfo _configInfo;

        protected ContextValuesValueInjector(LambdaExpression lambda, MappingConfigInfo configInfo)
        {
            _lambda = lambda;
            _configInfo = configInfo;
        }

        public static IValueInjector Create(LambdaExpression lambda, MappingConfigInfo configInfo)
        {
            var requiredValues = ParametersAccessFinder.GetValuesRequiredBy(lambda);

            return Create(lambda, configInfo, requiredValues);
        }

        public static IValueInjector Create(
            LambdaExpression lambda,
            MappingConfigInfo configInfo,
            RequiredValuesSet requiredValues)
        {
            switch (requiredValues.ValuesCount)
            {
                case 0:
                    return new NullValueInjector(lambda);

                case 1:
                    return new SingleContextValueValueInjector(lambda, configInfo, requiredValues);

                default:
                    return new MultipleContextValuesValueInjector(lambda, configInfo, requiredValues);
            }
        }

        public abstract bool HasMappingContextParameter { get; }

        public abstract Expression Inject(Type[] contextTypes, IMemberMapperData mapperData);

        protected ValueInjectionContext CreateContext(Type[] contextTypes, IMemberMapperData mapperData)
        {
            var args = new ValueInjectionArgs(_lambda, _configInfo, contextTypes, mapperData);
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
                    _replacementFactory = ctx => ctx.GetElementIndex();
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

            public override bool HasMappingContextParameter => false;

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
                MappingConfigInfo configInfo,
                RequiredValuesSet requiredValues)
                : base(lambda, configInfo)
            {
                _lambdaBody = lambda.Body;
                _isInvocation = lambda.IsInvocation();
                _requiredValues = requiredValues;
            }

            private int RequiredValuesCount => _requiredValues.ValuesCount;

            public override bool HasMappingContextParameter => _requiredValues.Includes(MappingContext);

            public override Expression Inject(Type[] contextTypes, IMemberMapperData mapperData)
            {
                var replacements = _isInvocation
                    ? FixedSizeExpressionReplacementDictionary.WithEqualKeys(RequiredValuesCount)
                    : FixedSizeExpressionReplacementDictionary.WithEquivalentKeys(RequiredValuesCount);

                var context = CreateContext(contextTypes, mapperData);

                if (_requiredValues.Includes(MappingContext))
                {
                    replacements.Add(_requiredValues.MappingContext, context.GetMappingDataAccess());
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
                    replacements.Add(_requiredValues.ElementIndex, context.GetElementIndex());
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