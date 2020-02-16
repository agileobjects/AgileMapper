﻿namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
#if NET35
    using LinqExp = System.Linq.Expressions;
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;
    using static MappingRuleSet;

    internal delegate bool SourceTypeComparer(ITypePair typePair, ITypePair otherTypePair);
    
    internal delegate bool TargetTypeComparer(ITypePair typePair, ITypePair otherTypePair);

    internal class MappingConfigInfo : ITypePair
    {
        public static readonly MappingConfigInfo AllRuleSetsSourceTypesAndTargetTypes =
            AllRuleSetsAndSourceTypes(null).ForAllTargetTypes();

        private ConfiguredLambdaInfo _conditionLambda;
        private bool _negateCondition;
        private Dictionary<Type, object> _data;
        private IObjectMappingData _mappingData;
        private bool _isForSourceTypeOnly;

        public MappingConfigInfo(MapperContext mapperContext)
        {
            MapperContext = mapperContext;
        }

        #region Factory Methods

        public static MappingConfigInfo AllRuleSetsAndSourceTypes(MapperContext mapperContext)
            => new MappingConfigInfo(mapperContext).ForAllSourceTypes().ForAllRuleSets();

        #endregion

        public MapperContext MapperContext { get; }

        public UserConfigurationSet UserConfigurations => MapperContext.UserConfigurations;

        public bool HasSameTypesAs(UserConfiguredItemBase userConfiguredItem)
            => HasSameSourceTypeAs(userConfiguredItem) && HasSameTargetTypeAs(userConfiguredItem);

        public bool HasCompatibleTypes(ITypePair otherTypePair)
            => this.HasTypesCompatibleWith(otherTypePair);

        public Type SourceType { get; private set; }

        public MappingConfigInfo ForAllSourceTypes() => ForSourceType(Constants.AllTypes);

        public MappingConfigInfo ForSourceTypeOnly()
        {
            if (SourceType.IsSealed())
            {
                throw new MappingConfigurationException(
                    $"Source type {SourceType.GetFriendlyName()} is sealed, so cannot have derived types");
            }

            _isForSourceTypeOnly = true;
            return this;
        }

        public MappingConfigInfo ForSourceType<TSource>() => ForSourceType(typeof(TSource));

        public MappingConfigInfo ForSourceType(Type sourceType)
        {
            SourceType = sourceType;
            return this;
        }

        bool ITypePair.IsForSourceType(ITypePair typePair)
        {
            if (_isForSourceTypeOnly)
            {
                return this.IsForAllSourceTypes() || HasSameSourceTypeAs(typePair);
            }

            return Get<SourceTypeComparer>()?.Invoke(this, typePair) ??
                   this.IsForSourceType(typePair);
        }

        public bool HasSameSourceTypeAs(UserConfiguredItemBase userConfiguredItem)
            => HasSameSourceTypeAs(userConfiguredItem.ConfigInfo);

        private bool HasSameSourceTypeAs(ITypePair typePair) => typePair.SourceType == SourceType;

        public Type TargetType { get; private set; }
        
        public bool IsForAllTargetTypes() => TargetType == typeof(object);

        public MappingConfigInfo ForAllTargetTypes() => ForTargetType<object>();

        public MappingConfigInfo ForTargetType<TTarget>() => ForTargetType(typeof(TTarget));

        public MappingConfigInfo ForTargetType(Type targetType)
        {
            TargetType = targetType;
            return this;
        }

        bool ITypePair.IsForTargetType(ITypePair typePair)
        {
            return Get<TargetTypeComparer>()?.Invoke(this, typePair) ??
                   this.IsForTargetType(typePair);
        }

        public bool HasSameTargetTypeAs(UserConfiguredItemBase userConfiguredItem)
            => TargetType == userConfiguredItem.TargetType;

        public MappingRuleSet RuleSet { get; private set; }

        public MappingConfigInfo ForAllRuleSets() => ForRuleSet(All);

        public MappingConfigInfo ForRuleSet(string ruleSetName)
            => ForRuleSet(MapperContext.RuleSets.GetByName(ruleSetName));

        public MappingConfigInfo ForRuleSet(MappingRuleSet ruleSet)
        {
            RuleSet = ruleSet;
            return this;
        }

        public bool IsForAllRuleSets => IsFor(All);

        public bool IsFor(MappingRuleSet mappingRuleSet)
            => (RuleSet == All) || (mappingRuleSet == All) || (mappingRuleSet == RuleSet);

        public Type SourceValueType { get; private set; }

        public MappingConfigInfo ForSourceValueType(Type sourceValueType)
        {
            SourceValueType = sourceValueType;
            return this;
        }

        public void ThrowIfSourceTypeUnconvertible(Type targetValueType)
            => MapperContext.ValueConverters.ThrowIfUnconvertible(SourceValueType, targetValueType);

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

        private void ErrorIfConditionHasTypeTest(LambdaExpression conditionLambda)
        {
            if ((SourceType?.IsInterface() == true) ||
                 TypeTestFinder.HasNoTypeTest(conditionLambda))
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

        public string GetConditionDescription(MappingConfigInfo configInfo)
            => _conditionLambda.GetDescription(configInfo);

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
                condition = condition.Negate();
            }

            var targetCanBeNull = position.IsPriorToObjectCreation(targetMember);

            var conditionNestedAccessesChecks = mapperData
                .GetNestedAccessChecksFor(condition, targetCanBeNull);

            if (conditionNestedAccessesChecks != null)
            {
                condition = Expression.AndAlso(conditionNestedAccessesChecks, condition);
            }

            return condition;
        }

        #endregion

        public T Get<T>()
        {
            if (_data == null)
            {
                return default;
            }

            return _data.TryGetValue(typeof(T), out var value) ? (T)value : default;
        }

        public MappingConfigInfo Set<T>(T value)
        {
            Data[typeof(T)] = value;
            return this;
        }

        private Dictionary<Type, object> Data => (_data ?? (_data = new Dictionary<Type, object>()));

        public IObjectMappingData ToMappingData<TSource, TTarget>()
        {
            if ((_mappingData != null) &&
                 _mappingData.MappingTypes.Equals(MappingTypes<TSource, TTarget>.Fixed))
            {
                return _mappingData;
            }

            var ruleSet = IsForAllRuleSets
                ? MapperContext.RuleSets.CreateNew
                : RuleSet;

            var mappingContext = new SimpleMappingContext(ruleSet, MapperContext);

            _mappingData = ObjectMappingDataFactory
                .ForRootFixedTypes<TSource, TTarget>(mappingContext, createMapper: false);

            return _mappingData;
        }

        public IQualifiedMemberContext ToMemberContext(QualifiedMember targetMember = null)
        {
            if (targetMember == null)
            {
                targetMember = QualifiedMember.CreateRoot(Member.RootTarget(TargetType), MapperContext);
            }

            return new QualifiedMemberContext(
                RuleSet,
                SourceType,
                TargetType,
                targetMember,
                parent: null,
                mapperContext: MapperContext);
        }

        public MappingConfigInfo Copy()
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