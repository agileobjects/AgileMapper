namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
    using LinqExp = System.Linq.Expressions;
#else
    using System.Linq.Expressions;
#endif
    using Api.Configuration;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    internal abstract class ConfiguredSourceValueFilter : UserConfiguredItemBase
    {
        private static readonly Expression _true = Expression.Constant(true, typeof(bool));
        private static readonly Expression _false = Expression.Constant(false, typeof(bool));

        protected ConfiguredSourceValueFilter(MappingConfigInfo configInfo)
            : base(configInfo)
        {
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
            private readonly Expression _valuesFilter;
            private readonly FilterCondition _filterCondition;

            public SingleConditionConfiguredSourceValueFilter(
                MappingConfigInfo configInfo,
                Expression valuesFilter,
                FilterCondition filterCondition)
                : base(configInfo)
            {
                _valuesFilter = valuesFilter;
                _filterCondition = filterCondition;
            }

            protected override Expression GetFilterExpression(Expression sourceValue, ref bool hasFixedValueOperands)
            {
                return _valuesFilter.Replace(
                    _filterCondition.Filter,
                    _filterCondition.GetConditionReplacement(sourceValue, ref hasFixedValueOperands));
            }
        }

        private class MultipleConditionConfiguredSourceValueFilter : ConfiguredSourceValueFilter
        {
            private readonly Expression _valuesFilter;
            private readonly IList<FilterCondition> _filterConditions;
            private readonly Dictionary<Expression, Expression> _conditionReplacements;

            public MultipleConditionConfiguredSourceValueFilter(
                MappingConfigInfo configInfo,
                Expression valuesFilter,
                IList<FilterCondition> filterConditions)
                : base(configInfo)
            {
                _valuesFilter = valuesFilter;
                _filterConditions = filterConditions;
                _conditionReplacements = filterConditions.ToDictionary(fc => fc.Filter, fc => default(Expression));
            }

            protected override Expression GetFilterExpression(Expression sourceValue, ref bool hasFixedValueOperands)
            {
                foreach (var filterCondition in _filterConditions)
                {
                    _conditionReplacements[filterCondition.Filter] =
                        filterCondition.GetConditionReplacement(sourceValue, ref hasFixedValueOperands);
                }

                return _valuesFilter.Replace(_conditionReplacements);
            }
        }

        #region Helper Classes

        private class FilterCondition
        {
            private readonly LambdaExpression _filterLambda;
            private readonly Type _filteredValueType;
            private readonly bool _appliesToAllSources;
            private readonly bool _filteredValueTypeIsNullable;

            private FilterCondition(
                MethodCallExpression filterCreationCall,
                Type filteredValueType)
            {
                Filter = filterCreationCall;

                var filterLambda = filterCreationCall.Arguments.First();

                if (filterLambda.NodeType == ExpressionType.Quote)
                {
                    filterLambda = ((UnaryExpression)filterLambda).Operand;
                }

                _filterLambda = (LambdaExpression)filterLambda;
                _filteredValueType = filteredValueType;
                _appliesToAllSources = filteredValueType == typeof(object);

                if (!_appliesToAllSources)
                {
                    _filteredValueTypeIsNullable = filteredValueType.IsNullableType();
                }
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
                => _filterLambda.ReplaceParameterWith(sourceValue);

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