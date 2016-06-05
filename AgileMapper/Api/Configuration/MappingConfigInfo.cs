namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal class MappingConfigInfo
    {
        private static readonly Type _allSourceTypes = typeof(MappingConfigInfo);
        private static readonly string _allRuleSets = Guid.NewGuid().ToString();

        private Type _sourceType;
        private Type _sourceValueType;
        private string _mappingRuleSetName;
        private ConfiguredLambdaInfo _conditionLambda;
        private bool _negateCondition;

        public MappingConfigInfo(MapperContext mapperContext)
        {
            MapperContext = mapperContext;
        }

        public GlobalContext GlobalContext => MapperContext.GlobalContext;

        public MapperContext MapperContext { get; }

        public MappingConfigInfo ForAllSourceTypes() => ForSourceType(_allSourceTypes);

        public MappingConfigInfo ForSourceType<TSource>() => ForSourceType(typeof(TSource));

        public MappingConfigInfo ForSourceType(Type sourceType)
        {
            _sourceType = sourceType;
            return this;
        }

        public bool IsForSourceType(Type sourceType)
            => (_sourceType == _allSourceTypes) || _sourceType.IsAssignableFrom(sourceType);

        public MappingConfigInfo ForAllRuleSets() => ForRuleSet(_allRuleSets);

        public MappingConfigInfo ForRuleSet(string name)
        {
            _mappingRuleSetName = name;
            return this;
        }

        public bool IsForRuleSet(string mappingRuleSetName)
        {
            return (_mappingRuleSetName == _allRuleSets) ||
                (mappingRuleSetName == _mappingRuleSetName);
        }

        public MappingConfigInfo ForSourceValueType(Type sourceValueType)
        {
            _sourceValueType = sourceValueType;
            return this;
        }

        public void ThrowIfSourceTypeDoesNotMatch<TTargetValue>()
        {
            MapperContext.ValueConverters.ThrowIfUnconvertible(_sourceValueType, typeof(TTargetValue));
        }

        public void AddCondition(LambdaExpression conditionLambda)
        {
            _conditionLambda = ConfiguredLambdaInfo.For(conditionLambda);
        }

        public void NegateCondition()
        {
            if (_conditionLambda != null)
            {
                _negateCondition = true;
            }
        }

        public Expression GetConditionOrNull(IMemberMappingContext context)
        {
            if (_conditionLambda == null)
            {
                return null;
            }

            var contextualisedCondition = _conditionLambda.GetBody(context);

            if (_negateCondition)
            {
                contextualisedCondition = Expression.Not(contextualisedCondition);
            }

            return context.WrapInTry(contextualisedCondition);
        }
    }
}