namespace AgileObjects.AgileMapper.Configuration.MemberIgnores.SourceValueFilters
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Api.Configuration;
    using Extensions.Internal;
    using Members.MemberExtensions;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
#if NET35
    using LinqExp = System.Linq.Expressions;
#endif
    using static FilterConstants;

    internal class FilterCondition
    {
        private readonly Type _filteredValueType;
        private readonly bool _appliesToAllSources;
        private readonly bool _filteredValueTypeIsNullable;
        private readonly Expression _filterParameter;
        private readonly Expression _filterExpression;
        private readonly Expression _filterNestedAccessChecks;

        private FilterCondition(MethodCallExpression filterCreationCall, Type filteredValueType)
        {
            Filter = filterCreationCall;
            _filteredValueType = filteredValueType;
            _appliesToAllSources = filteredValueType == typeof(object);

            if (!_appliesToAllSources)
            {
                _filteredValueTypeIsNullable = filteredValueType.IsNullableType();
            }

            var filterArgument = filterCreationCall.Arguments.First();
#if NET35
            var filterLinqLambda = (LinqExp.LambdaExpression)((ConstantExpression)filterArgument).Value;
            var filterLambda = filterLinqLambda.ToDlrExpression();
#else
            if (filterArgument.NodeType == ExpressionType.Quote)
            {
                filterArgument = ((UnaryExpression)filterArgument).Operand;
            }

            var filterLambda = (LambdaExpression)filterArgument;
#endif
            _filterParameter = filterLambda.Parameters.First();
            _filterExpression = filterLambda.Body;

            _filterNestedAccessChecks = NestedAccessChecksFactory
                .GetNestedAccessChecksFor(_filterExpression, invertChecks: true);
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
                return False;
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
}