namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal class MappingConfigInfo
    {
        private static readonly Type _allSourceTypes = typeof(MappingConfigInfo);
        private const string AllRuleSets = "*";

        private Type _sourceType;
        private Type _targetType;
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

        public GlobalContext GlobalContext => MapperContext.GlobalContext;

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
            _targetType = targetType;
            return this;
        }

        public bool HasSameSourceTypeAs(MappingConfigInfo otherConfigInfo) => _sourceType == otherConfigInfo._sourceType;

        public bool IsForSourceType(MappingConfigInfo otherConfigInfo) => IsForSourceType(otherConfigInfo._sourceType);

        public bool IsForSourceType(Type sourceType)
            => (_sourceType == _allSourceTypes) || _sourceType.IsAssignableFrom(sourceType);

        public bool HasSameTargetTypeAs(MappingConfigInfo otherConfigInfo) => _targetType == otherConfigInfo._targetType;

        public bool IsForTargetType(MappingConfigInfo otherConfigInfo) => IsForTargetType(otherConfigInfo._targetType);

        public bool IsForTargetType(Type targetType) => _targetType.IsAssignableFrom(targetType);

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

        public Expression GetConditionOrNull(IMemberMappingContext context)
        {
            if (!HasCondition)
            {
                return null;
            }

            var contextualisedCondition = _conditionLambda.GetBody(context);

            if (_negateCondition)
            {
                contextualisedCondition = Expression.Not(contextualisedCondition);
            }

            return contextualisedCondition;
        }

        #endregion

        public QualifiedMember GetTargetMemberFrom(LambdaExpression lambda)
            => lambda?.Body.ToTargetMember(GlobalContext.MemberFinder, MapperContext.NamingSettings);

        public MappingConfigInfo CloneForContinuation()
        {
            return new MappingConfigInfo(MapperContext)
            {
                _sourceType = _sourceType,
                _targetType = _targetType,
                _sourceValueType = _sourceValueType,
                _mappingRuleSetName = _mappingRuleSetName
            };
        }
    }
}