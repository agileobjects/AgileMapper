namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Extensions.Internal;
    using Members;
    using ObjectPopulation;
    using ReadableExpressions;
#if NET35
    using LinqExp = System.Linq.Expressions;
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class MappingConfigInfo : ITypePair
    {
        private static readonly MappingRuleSet _allRuleSets = new MappingRuleSet("*", null, null, null, null, null, null);

        public static readonly MappingConfigInfo AllRuleSetsSourceTypesAndTargetTypes =
            AllRuleSetsAndSourceTypes(null).ForAllTargetTypes();

        private ConfiguredLambdaInfo _conditionLambda;
        private bool _negateCondition;
        private Dictionary<Type, object> _data;

        public MappingConfigInfo(MapperContext mapperContext)
        {
            MapperContext = mapperContext;
        }

        #region Factory Methods

        public static MappingConfigInfo AllRuleSetsAndSourceTypes(MapperContext mapperContext)
            => new MappingConfigInfo(mapperContext).ForAllSourceTypes().ForAllRuleSets();

        #endregion

        public MapperContext MapperContext { get; }

        public IMapper Mapper => MapperContext.Mapper;

        public Type SourceType { get; private set; }

        public MappingConfigInfo ForAllSourceTypes() => ForSourceType(Constants.AllTypes);

        public MappingConfigInfo ForSourceType<TSource>() => ForSourceType(typeof(TSource));

        public MappingConfigInfo ForSourceType(Type sourceType)
        {
            SourceType = sourceType;
            return this;
        }

        public bool HasSameSourceTypeAs(MappingConfigInfo otherConfigInfo) => otherConfigInfo.SourceType == SourceType;

        public Type TargetType { get; private set; }

        public MappingConfigInfo ForAllTargetTypes() => ForTargetType<object>();

        public MappingConfigInfo ForTargetType<TTarget>() => ForTargetType(typeof(TTarget));

        public MappingConfigInfo ForTargetType(Type targetType)
        {
            TargetType = targetType;
            return this;
        }

        public bool HasSameTargetTypeAs(MappingConfigInfo otherConfigInfo) => TargetType == otherConfigInfo.TargetType;

        public bool HasCompatibleTypes(MappingConfigInfo otherConfigInfo)
            => ((ITypePair)this).HasCompatibleTypes(otherConfigInfo);

        public MappingRuleSet RuleSet { get; private set; }

        public MappingConfigInfo ForAllRuleSets() => ForRuleSet(_allRuleSets);

        public MappingConfigInfo ForRuleSet(string ruleSetName)
            => ForRuleSet(MapperContext.RuleSets.GetByName(ruleSetName));

        public MappingConfigInfo ForRuleSet(MappingRuleSet ruleSet)
        {
            RuleSet = ruleSet;
            return this;
        }

        public bool IsFor(MappingRuleSet mappingRuleSet)
            => (RuleSet == _allRuleSets) || (mappingRuleSet == _allRuleSets) || (mappingRuleSet == RuleSet);

        public Type SourceValueType { get; private set; }

        public MappingConfigInfo ForSourceValueType<TSourceValue>() => ForSourceValueType(typeof(TSourceValue));

        public MappingConfigInfo ForSourceValueType(Type sourceValueType)
        {
            SourceValueType = sourceValueType;
            return this;
        }

        public void ThrowIfSourceTypeUnconvertible<TTargetValue>()
            => MapperContext.ValueConverters.ThrowIfUnconvertible(SourceValueType, typeof(TTargetValue));

        #region Conditions

        public bool ConditionUsesMappingDataObjectParameter
            => HasCondition && _conditionLambda.UsesMappingDataObjectParameter;

        public bool HasCondition => _conditionLambda != null;

        public bool ConditionSupports(MappingRuleSet ruleSet) => _conditionLambda.Supports(ruleSet);

#if NET35
        public void AddConditionOrThrow(LinqExp.LambdaExpression conditionLambda)
            => AddConditionOrThrow(conditionLambda.ToDlrExpression());
#endif
        public void AddConditionOrThrow(LambdaExpression conditionLambda)
        {
            ErrorIfConditionHasTypeTest(conditionLambda);
            FixEnumComparisonsIfNecessary(ref conditionLambda);

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

        private static void FixEnumComparisonsIfNecessary(ref LambdaExpression conditionLambda)
        {
            conditionLambda = EnumComparisonFixer.Check(conditionLambda);
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
                return null;
            }

            var condition = _conditionLambda.GetBody(mapperData, position, targetMember);

            if (_negateCondition)
            {
                condition = Expression.Not(condition);
            }

            var targetCanBeNull = position.IsPriorToObjectCreation(targetMember);

            var conditionNestedAccessesChecks = mapperData
                .GetExpressionInfoFor(condition, targetCanBeNull)
                .NestedAccessChecks;

            if (conditionNestedAccessesChecks != null)
            {
                condition = Expression.AndAlso(conditionNestedAccessesChecks, condition);
            }

            return condition;
        }

        #endregion

        public T Get<T>() => Data.TryGetValue(typeof(T), out var value) ? (T)value : default(T);

        public MappingConfigInfo Set<T>(T value)
        {
            Data[typeof(T)] = value;
            return this;
        }

        private Dictionary<Type, object> Data => (_data ?? (_data = new Dictionary<Type, object>()));

        public IBasicMapperData ToMapperData()
        {
            var dummyTargetMember = QualifiedMember
                .From(Member.RootTarget(TargetType), MapperContext);

            return new BasicMapperData(
                RuleSet,
                SourceType,
                TargetType,
                dummyTargetMember);
        }

        public MappingConfigInfo Clone()
        {
            var cloned = new MappingConfigInfo(MapperContext)
            {
                SourceType = SourceType,
                TargetType = TargetType,
                SourceValueType = SourceValueType,
                RuleSet = RuleSet
            };

            if (_data == null)
            {
                return cloned;
            }

            foreach (var itemByType in _data)
            {
                cloned.Data.Add(itemByType.Key, itemByType.Value);
            }

            return cloned;
        }

        private class TypeTestFinder : QuickUnwindExpressionVisitor
        {
            protected override bool QuickUnwind => TypeTestExists;

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