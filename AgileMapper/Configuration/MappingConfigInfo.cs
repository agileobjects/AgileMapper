namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Globalization;
    using System.Linq.Expressions;
    using Extensions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Members;
    using ObjectPopulation;
    using ReadableExpressions;
    using NetStandardPolyfills;

    internal class MappingConfigInfo
    {
        private static readonly Type _allSourceTypes = typeof(MappingConfigInfo);
        private static readonly MappingRuleSet _allRuleSets = new MappingRuleSet("*", true, null, null, null);

        private MappingRuleSet _mappingRuleSet;
        private Type _sourceType;
        private Type _targetType;
        private Type _sourceValueType;
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

        public bool HasSameTargetTypeAs(MappingConfigInfo otherConfigInfo) => _targetType == otherConfigInfo._targetType;

        public bool HasCompatibleTypes(MappingConfigInfo otherConfigInfo)
            => HasCompatibleTypes(otherConfigInfo._sourceType, otherConfigInfo._targetType);

        public bool HasCompatibleTypes(IBasicMapperData mapperData)
            => HasCompatibleTypes(mapperData.SourceType, mapperData.TargetType);

        public bool HasCompatibleTypes(Type sourceType, Type targetType)
        {
            return IsForSourceType(sourceType) &&
                (_targetType.IsAssignableFrom(targetType) || targetType.IsAssignableFrom(_targetType));
        }

        public bool IsForSourceType(MappingConfigInfo otherConfigInfo) => IsForSourceType(otherConfigInfo._sourceType);

        private bool IsForSourceType(Type sourceType)
            => (_sourceType == _allSourceTypes) || _sourceType.IsAssignableFrom(sourceType);

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

        public bool ConditionUsesMappingDataObjectParameter
            => HasCondition && _conditionLambda.UsesMappingDataObjectParameter;

        public bool HasCondition => _conditionLambda != null;

        public void AddConditionOrThrow(LambdaExpression conditionLambda)
        {
            ErrorIfConditionHasTypeTest(conditionLambda);

            _conditionLambda = ConfiguredLambdaInfo.For(conditionLambda);
        }

        private static void ErrorIfConditionHasTypeTest(LambdaExpression conditionLambda)
        {
            if (TypeTestFinder.HasNoTypeTest(conditionLambda))
            {
                return;
            }

            var condition = conditionLambda.Body.ToReadableString();

            throw new MappingConfigurationException(string.Format(
                CultureInfo.InvariantCulture,
                "Instead of type testing in condition '{0}', configure for a more specific source or target type.",
                condition));
        }

        public void NegateCondition()
        {
            if (HasCondition)
            {
                _negateCondition = true;
            }
        }

        public Expression GetConditionOrNull(
            IMemberMapperData mapperData,
            CallbackPosition position,
            QualifiedMember targetMember)
        {
            if (!HasCondition)
            {
                return GetTypeCheckConditionOrNull(mapperData);
            }

            var condition = _conditionLambda.GetBody(mapperData, position, targetMember);

            if (_negateCondition)
            {
                condition = Expression.Not(condition);
            }

            var conditionNestedAccessesChecks = mapperData
                .GetNestedAccessesIn(condition, targetCanBeNull: true) // TODO: Use position.IsPriorToObjectCreation(targetMember)
                .GetIsNotDefaultComparisonsOrNull();

            if (conditionNestedAccessesChecks != null)
            {
                condition = Expression.AndAlso(conditionNestedAccessesChecks, condition);
            }

            var typeCheck = GetTypeCheckConditionOrNull(mapperData);

            if (typeCheck != null)
            {
                condition = Expression.AndAlso(typeCheck, condition);
            }

            return condition;
        }

        private Expression GetTypeCheckConditionOrNull(IMemberMapperData mapperData)
        {
            var sourceType = (_sourceType == _allSourceTypes) ? typeof(object) : _sourceType;
            var contextTypes = new[] { sourceType, _targetType };
            var context = mapperData.GetAppropriateMappingContext(contextTypes);

            if (!_targetType.IsDerivedFrom(context.TargetType))
            {
                return null;
            }

            var contextAccess = mapperData.GetAppropriateMappingContextAccess(contextTypes);

            if (contextAccess == mapperData.MappingDataObject)
            {
                return null;
            }

            var targetAccess = mapperData.GetTargetAccess(contextAccess, _targetType);
            var targetAccessNotNull = targetAccess.GetIsNotDefaultComparison();

            return targetAccessNotNull;
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

        private class TypeTestFinder : ExpressionVisitor
        {
            private bool TypeTestExists { get; set; }

            public static bool HasNoTypeTest(LambdaExpression lambda)
            {
                var typesFinder = new TypeTestFinder();

                typesFinder.Visit(lambda.Body);

                return !typesFinder.TypeTestExists;
            }

            protected override Expression VisitUnary(UnaryExpression unary)
            {
                if (unary.NodeType == ExpressionType.TypeAs)
                {
                    TypeTestExists = true;
                    return unary;
                }

                return base.VisitUnary(unary);
            }

            protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinary)
            {
                TypeTestExists = true;
                return typeBinary;
            }
        }
    }
}