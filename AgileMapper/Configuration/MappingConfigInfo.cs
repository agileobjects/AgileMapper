namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal class MappingConfigInfo
    {
        private static readonly object _configurationSync = new object();
        private static readonly Type _allSourceTypes = typeof(MappingConfigInfo);
        private static readonly MappingRuleSet _allRuleSets = new MappingRuleSet("*", null, null, null, null);

        private Type _sourceType;
        private Type _targetType;
        private Type _sourceValueType;
        private MappingRuleSet _mappingRuleSet;
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

        public MappingConfigInfo ForAllRuleSets() => ForRuleSet(_allRuleSets);

        public MappingConfigInfo ForRuleSet(string ruleSetName)
        {
            _mappingRuleSet = MapperContext.RuleSets.GetByName(ruleSetName);
            return this;
        }

        public MappingConfigInfo ForRuleSet(MappingRuleSet ruleSet)
        {
            _mappingRuleSet = ruleSet;
            return this;
        }

        public bool IsFor(MappingRuleSet mappingRuleSet)
            => (_mappingRuleSet == _allRuleSets) || (mappingRuleSet == _mappingRuleSet);

        public MappingConfigInfo ForSourceValueType<TSourceValue>() => ForSourceValueType(typeof(TSourceValue));

        public MappingConfigInfo ForSourceValueType(Type sourceValueType)
        {
            _sourceValueType = sourceValueType;
            return this;
        }

        public void ThrowIfSourceTypeUnconvertible<TTargetValue>()
            => MapperContext.ValueConverters.ThrowIfUnconvertible(_sourceValueType, typeof(TTargetValue));

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

        public Expression GetConditionOrNull<TSource, TTarget>()
        {
            if (!HasCondition)
            {
                return null;
            }

            using (var stubMappingContext = new MappingContext(_mappingRuleSet, MapperContext))
            {
                MemberMapperData mapperData;

                lock (_configurationSync)
                {
                    MapperContext.UserConfigurations.DerivedTypePairs.Configuring = true;

                    mapperData = stubMappingContext
                        .CreateRootMapperCreationData(default(TSource), default(TTarget))
                        .MapperData;

                    MapperContext.UserConfigurations.DerivedTypePairs.Configuring = false;
                }

                var condition = GetConditionOrNull(mapperData);
                condition = mapperData.ReplaceTypedParameterWithUntyped(condition);

                return condition;
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

        public MappingConfigInfo CloneForContinuation()
        {
            return new MappingConfigInfo(MapperContext)
            {
                _sourceType = _sourceType,
                _targetType = _targetType,
                _sourceValueType = _sourceValueType,
                _mappingRuleSet = _mappingRuleSet
            };
        }
    }
}