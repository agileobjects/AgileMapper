namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;
    using ReadableExpressions;

    internal class MappingConfigInfo
    {
        private static readonly Type _allSourceTypes = typeof(MappingConfigInfo);
        private const string AllRuleSets = "*";

        private Type _sourceType;
        private Type _sourceValueType;
        private string _mappingRuleSetName;
        private ConfiguredLambdaInfo _conditionLambda;
        private bool _negateCondition;

        public MappingConfigInfo(MapperContext mapperContext)
        {
            MapperContext = mapperContext;
        }

        #region Factory Methods

        public static MappingConfigInfo AllRuleSetsAndSourceTypes(MapperContext mapperContext)
            => new MappingConfigInfo(mapperContext).ForAllRuleSets().ForAllSourceTypes();

        public static MappingConfigInfo AllRuleSetsSourceTypesAndTargetTypes(MapperContext mapperContext)
            => AllRuleSetsAndSourceTypes(mapperContext).ForAllTargetTypes();

        #endregion

        public MapperContext MapperContext { get; }

        public MappingConfigInfo ForAllSourceTypes() => ForSourceType(_allSourceTypes);

        public MappingConfigInfo ForSourceType<TSource>() => ForSourceType(typeof(TSource));

        public MappingConfigInfo ForSourceType(Type sourceType)
        {
            _sourceType = sourceType;
            return this;
        }

        public MappingConfigInfo ForAllTargetTypes() => ForTargetType<object>();

        public MappingConfigInfo ForTargetType<TTarget>() => ForTargetType(typeof(TTarget));

        public MappingConfigInfo ForTargetType(Type targetType)
        {
            TargetType = targetType;
            return this;
        }

        internal Type TargetType { get; private set; }

        public bool HasSameSourceTypeAs(MappingConfigInfo otherConfigInfo) => _sourceType == otherConfigInfo._sourceType;

        public bool IsForSourceType(MappingConfigInfo otherConfigInfo) => IsForSourceType(otherConfigInfo._sourceType);

        public bool IsForSourceType(Type sourceType)
            => (_sourceType == _allSourceTypes) || _sourceType.IsAssignableFrom(sourceType);

        public bool HasSameTargetTypeAs(MappingConfigInfo otherConfigInfo) => TargetType == otherConfigInfo.TargetType;

        public bool IsForTargetType(MappingConfigInfo otherConfigInfo) => IsForTargetType(otherConfigInfo.TargetType);

        public bool IsForTargetType(Type targetType) => TargetType.IsAssignableFrom(targetType);

        public MappingConfigInfo ForAllRuleSets() => ForRuleSet(AllRuleSets);

        public MappingConfigInfo ForRuleSet(string name)
        {
            _mappingRuleSetName = name;
            return this;
        }

        public bool IsForRuleSet(string mappingRuleSetName)
        {
            return (_mappingRuleSetName == AllRuleSets) ||
                (mappingRuleSetName == _mappingRuleSetName);
        }

        public MappingConfigInfo ForSourceValueType<TSourceValue>() => ForSourceValueType(typeof(TSourceValue));

        public MappingConfigInfo ForSourceValueType(Type sourceValueType)
        {
            _sourceValueType = sourceValueType;
            return this;
        }

        public void ThrowIfSourceTypeDoesNotMatch<TTargetValue>()
        {
            MapperContext.ValueConverters.ThrowIfUnconvertible(_sourceValueType, typeof(TTargetValue));
        }

        #region Conditions

        public bool HasCondition => _conditionLambda != null;

        public void AddCondition(LambdaExpression conditionLambda)
        {
            _conditionLambda = ConfiguredLambdaInfo.For(conditionLambda);
        }

        public void NegateCondition()
        {
            if (HasCondition)
            {
                _negateCondition = true;
            }
        }

        public Expression GetConditionOrNull(MemberMapperData mapperData)
        {
            if (!HasCondition)
            {
                return null;
            }

            var contextualisedCondition = _conditionLambda.GetBody(mapperData);

            if (_negateCondition)
            {
                contextualisedCondition = Expression.Not(contextualisedCondition);
            }

            return contextualisedCondition;
        }

        #endregion

        public QualifiedMember GetTargetMemberFrom(LambdaExpression lambda)
        {
            var targetMember = lambda.Body.ToTargetMember(
                GlobalContext.Instance.MemberFinder,
                MapperContext.WithDefaultNamingSettings);

            if (targetMember != null)
            {
                return targetMember;
            }

            throw new MappingConfigurationException(
                $"Target member {lambda.Body.ToReadableString()} is not writeable.");
        }

        public MappingConfigInfo CloneForContinuation()
        {
            return new MappingConfigInfo(MapperContext)
            {
                _sourceType = _sourceType,
                TargetType = TargetType,
                _sourceValueType = _sourceValueType,
                _mappingRuleSetName = _mappingRuleSetName
            };
        }
    }
}