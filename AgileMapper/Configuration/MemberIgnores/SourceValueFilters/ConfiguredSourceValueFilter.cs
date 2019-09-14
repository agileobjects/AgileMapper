namespace AgileObjects.AgileMapper.Configuration.MemberIgnores.SourceValueFilters
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
    using LinqExp = System.Linq.Expressions;
#else
    using System.Linq.Expressions;
#endif
    using Api.Configuration;
    using Extensions.Internal;
    using Members;
    using ReadableExpressions;
    using static Extensions.Internal.ExpressionEvaluation;
    using static FilterConstants;

    internal abstract class ConfiguredSourceValueFilter :
        UserConfiguredItemBase,
        IPotentialAutoCreatedItem
#if NET35
        , IComparable<ConfiguredSourceValueFilter>
#endif
    {
        protected ConfiguredSourceValueFilter(MappingConfigInfo configInfo, Expression valuesFilter)
            : base(configInfo)
        {
            ValuesFilter = valuesFilter;
        }

        #region Factory Methods
#if NET35
        public static ConfiguredSourceValueFilter Create(
            MappingConfigInfo configInfo,
            LinqExp.Expression<Func<SourceValueFilterSpecifier, bool>> valuesFilter)
        {
            return Create(configInfo, valuesFilter.ToDlrExpression());
        }
#endif
        public static ConfiguredSourceValueFilter Create(
            MappingConfigInfo configInfo,
            Expression<Func<SourceValueFilterSpecifier, bool>> valuesFilter)
        {
            var filterConditions = FilterCondition.GetConditions(valuesFilter);

            if (filterConditions.None())
            {
                throw new MappingConfigurationException("At least one source filter must be specified.");
            }

            if (filterConditions.HasOne())
            {
                return new SingleConditionConfiguredSourceValueFilter(
                    configInfo,
                    valuesFilter.Body,
                    filterConditions.First());
            }

            return new MultipleConditionConfiguredSourceValueFilter(
                configInfo,
                valuesFilter.Body,
                filterConditions);
        }

        #endregion

        protected Expression ValuesFilter { get; }

        public override bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
            => base.ConflictsWith(otherConfiguredItem) && FiltersAreTheSame((ConfiguredSourceValueFilter)otherConfiguredItem);

        private bool FiltersAreTheSame(ConfiguredSourceValueFilter otherSourceValueFilter)
            => AreEqual(ValuesFilter, otherSourceValueFilter.ValuesFilter);

        public string GetConflictMessage()
        {
            var filterDescription = ValuesFilter.ToReadableString(o => o.UseExplicitGenericParameters);

            return $"Source filter '{filterDescription}' has already been configured";
        }

        public bool AppliesTo(Type sourceValueType, IBasicMapperData mapperData)
            => AppliesTo(mapperData) && Filters(sourceValueType);

        protected abstract bool Filters(Type valueType);

        public Expression GetConditionOrNull(Expression sourceValue, IMemberMapperData mapperData)
        {
            var hasFixedValueOperands = false;
            var filterExpression = GetFilterExpression(sourceValue, ref hasFixedValueOperands);

            if (hasFixedValueOperands)
            {
                filterExpression = FilterOptimiser.Optimise(filterExpression);
            }

            if (filterExpression == False)
            {
                return null;
            }

            var condition = GetConditionOrNull(mapperData);

            if (condition != null)
            {
                filterExpression = Expression.AndAlso(condition, filterExpression);
            }

            return filterExpression.Negate();
        }

        protected abstract Expression GetFilterExpression(Expression sourceValue, ref bool hasFixedValueOperands);

        #region IPotentialAutoCreatedItem Members

        public bool WasAutoCreated { get; protected set; }

        public abstract IPotentialAutoCreatedItem Clone();

        public bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedItem)
        {
            var otherSourceValueFilter = (ConfiguredSourceValueFilter)autoCreatedItem;

            return otherSourceValueFilter.HasOverlappingTypes(this) && FiltersAreTheSame(otherSourceValueFilter);
        }

        #endregion

#if NET35
        int IComparable<ConfiguredSourceValueFilter>.CompareTo(ConfiguredSourceValueFilter other)
            => DoComparisonTo(other);
#endif

        private class SingleConditionConfiguredSourceValueFilter : ConfiguredSourceValueFilter
        {
            private readonly FilterCondition _filterCondition;

            public SingleConditionConfiguredSourceValueFilter(
                MappingConfigInfo configInfo,
                Expression valuesFilter,
                FilterCondition filterCondition)
                : base(configInfo, valuesFilter)
            {
                _filterCondition = filterCondition;
            }

            protected override bool Filters(Type valueType) => _filterCondition.Filters(valueType);

            protected override Expression GetFilterExpression(Expression sourceValue, ref bool hasFixedValueOperands)
            {
                return ValuesFilter.Replace(
                    _filterCondition.Filter,
                    _filterCondition.GetConditionReplacement(sourceValue, ref hasFixedValueOperands));
            }

            #region IPotentialAutoCreatedItem Members

            public override IPotentialAutoCreatedItem Clone()
            {
                return new SingleConditionConfiguredSourceValueFilter(ConfigInfo, ValuesFilter, _filterCondition)
                {
                    WasAutoCreated = true
                };
            }

            #endregion
        }

        private class MultipleConditionConfiguredSourceValueFilter : ConfiguredSourceValueFilter
        {
            private readonly IList<FilterCondition> _filterConditions;

            public MultipleConditionConfiguredSourceValueFilter(
                MappingConfigInfo configInfo,
                Expression valuesFilter,
                IList<FilterCondition> filterConditions)
                : base(configInfo, valuesFilter)
            {
                _filterConditions = filterConditions;
            }

            protected override bool Filters(Type valueType)
                => _filterConditions.Any(valueType, (vt, fc) => fc.Filters(vt));

            protected override Expression GetFilterExpression(Expression sourceValue, ref bool hasFixedValueOperands)
            {
                var conditionReplacements = new Dictionary<Expression, Expression>(_filterConditions.Count);

                foreach (var filterCondition in _filterConditions)
                {
                    conditionReplacements.Add(
                        filterCondition.Filter,
                        filterCondition.GetConditionReplacement(sourceValue, ref hasFixedValueOperands));
                }

                return ValuesFilter.Replace(conditionReplacements);
            }

            #region IPotentialAutoCreatedItem Members

            public override IPotentialAutoCreatedItem Clone()
            {
                return new MultipleConditionConfiguredSourceValueFilter(ConfigInfo, ValuesFilter, _filterConditions)
                {
                    WasAutoCreated = true
                };
            }

            #endregion
        }
    }
}