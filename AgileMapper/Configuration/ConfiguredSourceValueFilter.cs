namespace AgileObjects.AgileMapper.Configuration
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
    using NetStandardPolyfills;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal abstract class ConfiguredSourceValueFilter : UserConfiguredItemBase
    {
        private static readonly Expression _true = Expression.Constant(true, typeof(bool));
        private static readonly Expression _false = Expression.Constant(false, typeof(bool));

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
        {
            if (!base.ConflictsWith(otherConfiguredItem))
            {
                return false;
            }

            var otherSourceFilter = (ConfiguredSourceValueFilter)otherConfiguredItem;

            return ExpressionEvaluation.AreEqual(ValuesFilter, otherSourceFilter.ValuesFilter);
        }

        public string GetConflictMessage()
        {
            var filterDescription = ValuesFilter.ToReadableString(o => o.UseExplicitGenericParameters);

            return $"Source filter '{filterDescription}' has already been configured";
        }

        public bool AppliesTo(Type sourceValueType, IBasicMapperData mapperData)
            => AppliesTo(mapperData) && Filters(sourceValueType);

        protected abstract bool Filters(Type valueType);

        public Expression GetConditionOrNull(Expression sourceValue)
        {
            var hasFixedValueOperands = false;
            var filterExpression = GetFilterExpression(sourceValue, ref hasFixedValueOperands);

            if (hasFixedValueOperands)
            {
                filterExpression = FilterOptimiser.Optimise(filterExpression);
            }

            return (filterExpression != _false) ? filterExpression.Negate() : null;
        }

        protected abstract Expression GetFilterExpression(Expression sourceValue, ref bool hasFixedValueOperands);

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
        }

        #region Helper Classes

        private class FilterCondition
        {
            private readonly Type _filteredValueType;
            private readonly bool _appliesToAllSources;
            private readonly bool _filteredValueTypeIsNullable;
            private readonly Expression _filterParameter;
            private readonly Expression _filterExpression;
            private readonly Expression _filterNestedAccessChecks;

            private FilterCondition(
                MethodCallExpression filterCreationCall,
                Type filteredValueType)
            {
                Filter = filterCreationCall;
                _filteredValueType = filteredValueType;
                _appliesToAllSources = filteredValueType == typeof(object);

                if (!_appliesToAllSources)
                {
                    _filteredValueTypeIsNullable = filteredValueType.IsNullableType();
                }

                var filterArgument = filterCreationCall.Arguments.First();

                if (filterArgument.NodeType == ExpressionType.Quote)
                {
                    filterArgument = ((UnaryExpression)filterArgument).Operand;
                }

                var filterLambda = (LambdaExpression)filterArgument;

                _filterParameter = filterLambda.Parameters.First();
                _filterExpression = filterLambda.Body;

                _filterNestedAccessChecks = ExpressionInfoFinder
                    .Default
                    .FindIn(
                        _filterExpression,
                        checkMultiInvocations: false,
                        invertNestedAccessChecks: true)
                    .NestedAccessChecks;
            }

            #region Factory Method

            public static IList<FilterCondition> GetConditions(Expression filter)
            {
                var finder = new FilterConditionsFinder();

                finder.Visit(filter);

                return finder.Conditions;
            }

            #endregion

            public Expression Filter { get; }

            public bool Filters(Type valueType)
            {
                if (_appliesToAllSources)
                {
                    return true;
                }

                if (!_filteredValueTypeIsNullable)
                {
                    valueType = valueType.GetNonNullableType();
                }

                return valueType.IsAssignableTo(_filteredValueType);
            }

            public Expression GetConditionReplacement(Expression sourceValue, ref bool hasFixedValueOperands)
            {
                if (_appliesToAllSources)
                {
                    return GetFilterCondition(sourceValue.GetConversionToObject());
                }

                var sourceType = sourceValue.Type;

                if (!_filteredValueTypeIsNullable)
                {
                    sourceType = sourceType.GetNonNullableType();
                }

                if (!sourceType.IsAssignableTo(_filteredValueType))
                {
                    hasFixedValueOperands = true;
                    return _false;
                }

                if (sourceType != sourceValue.Type)
                {
                    sourceValue = sourceValue.GetNullableValueAccess();
                }

                return GetFilterCondition(sourceValue);
            }

            private Expression GetFilterCondition(Expression sourceValue)
            {
                var condition = ReplaceFilterParameter(_filterExpression, sourceValue);

                if (_filterNestedAccessChecks == null)
                {
                    return condition;
                }

                return Expression.OrElse(
                    ReplaceFilterParameter(_filterNestedAccessChecks, sourceValue),
                    condition);
            }

            private Expression ReplaceFilterParameter(Expression expression, Expression sourceValue)
                => expression.Replace(_filterParameter, sourceValue);

            private class FilterConditionsFinder : ExpressionVisitor
            {
                public FilterConditionsFinder()
                {
                    Conditions = new List<FilterCondition>();
                }

                public List<FilterCondition> Conditions { get; }

                protected override Expression VisitMethodCall(MethodCallExpression methodCall)
                {
                    if (methodCall.Method.DeclaringType != typeof(SourceValueFilterSpecifier))
                    {
                        return base.VisitMethodCall(methodCall);
                    }

                    var filterValueType = methodCall.Method.IsGenericMethod
                        ? methodCall.Method.GetGenericArguments().First()
                        : typeof(object);

                    Conditions.Add(new FilterCondition(methodCall, filterValueType));

                    return base.VisitMethodCall(methodCall);
                }
            }
        }

        private class FilterOptimiser : ExpressionVisitor
        {
            private bool _incomplete;

            public static Expression Optimise(Expression expression)
            {
                if (expression == _false)
                {
                    return expression;
                }

                var optimiser = new FilterOptimiser();

                do
                {
                    optimiser._incomplete = false;
                    expression = optimiser.VisitAndConvert(expression, nameof(FilterOptimiser));
                }
                while (optimiser._incomplete);

                return expression;
            }

            protected override Expression VisitUnary(UnaryExpression unary)
            {
                switch (unary.NodeType)
                {
                    case ExpressionType.Not when unary.Operand == _false:
                        _incomplete = true;
                        return _true;

                    case ExpressionType.Not when unary.Operand == _true:
                        _incomplete = true;
                        return _false;

                    default:
                        return base.VisitUnary(unary);
                }
            }

            protected override Expression VisitBinary(BinaryExpression binary)
            {
                switch (binary.NodeType)
                {
                    case ExpressionType.AndAlso when binary.Left == _false || binary.Right == _false:
                        _incomplete = true;
                        return _false;

                    case ExpressionType.OrElse when binary.Left == _true || binary.Right == _true:
                        _incomplete = true;
                        return _true;

                    case ExpressionType.AndAlso when binary.Left == _true:
                    case ExpressionType.OrElse when binary.Left == _false:
                        _incomplete = true;
                        // ReSharper disable once AssignNullToNotNullAttribute
                        return base.Visit(binary.Right);

                    case ExpressionType.AndAlso when binary.Right == _true:
                    case ExpressionType.OrElse when binary.Right == _false:
                        _incomplete = true;
                        // ReSharper disable once AssignNullToNotNullAttribute
                        return base.Visit(binary.Left);

                    default:
                        return base.VisitBinary(binary);
                }
            }
        }

        #endregion
    }
}