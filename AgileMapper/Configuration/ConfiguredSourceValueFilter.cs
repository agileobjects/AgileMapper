namespace AgileObjects.AgileMapper.Configuration
{
    using System;
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

    internal class ConfiguredSourceValueFilter : UserConfiguredItemBase
    {
        private static readonly Expression _true = Expression.Constant(true, typeof(bool));
        private static readonly Expression _false = Expression.Constant(false, typeof(bool));

        private readonly Expression _valuesFilterExpression;
#if NET35
        public ConfiguredSourceValueFilter(
            MappingConfigInfo configInfo,
            LinqExp.Expression<Func<SourceValueFilterSpecifier, bool>> valuesFilter)
            : this(configInfo, valuesFilter.ToDlrExpression())
        {
        }
#endif
        public ConfiguredSourceValueFilter(
            MappingConfigInfo configInfo,
            Expression<Func<SourceValueFilterSpecifier, bool>> valuesFilter)
            : base(configInfo)
        {
            _valuesFilterExpression = valuesFilter.Body;
        }

        public Expression GetConditionOrNull(Expression sourceValue)
        {
            var filterCondition = FilterFactory.Create(_valuesFilterExpression, sourceValue);

            return (filterCondition != _false) ? filterCondition : null;
        }

        #region Helper Classes

        private class FilterFactory : ExpressionVisitor
        {
            private readonly Expression _sourceValue;
            private bool _hasFixedValueOperands;

            private FilterFactory(Expression sourceValue)
            {
                _sourceValue = sourceValue;
            }

            public static Expression Create(Expression filter, Expression sourceValue)
            {
                var factory = new FilterFactory(sourceValue);

                var filterCondition = factory.VisitAndConvert(filter, nameof(FilterFactory));

                return factory._hasFixedValueOperands
                    ? FilterOptimiser.Optimise(filterCondition)
                    : filterCondition;
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCall)
            {
                if (methodCall.Method.DeclaringType != typeof(SourceValueFilterSpecifier))
                {
                    return base.VisitMethodCall(methodCall);
                }

                if (!methodCall.Method.IsGenericMethod)
                {
                    return GetFilter(methodCall, _sourceValue.GetConversionToObject());
                }

                var filterValueType = methodCall.Method.GetGenericArguments().First();
                
                if (!_sourceValue.Type.IsAssignableTo(filterValueType))
                {
                    _hasFixedValueOperands = true;
                    return _false;
                }

                return GetFilter(methodCall, _sourceValue);
            }

            private static Expression GetFilter(MethodCallExpression methodCall, Expression sourceValue)
            {
                var filterArgument = methodCall.Arguments.First();

                if (filterArgument.NodeType == ExpressionType.Quote)
                {
                    filterArgument = ((UnaryExpression)filterArgument).Operand;
                }

                return ((LambdaExpression)filterArgument).ReplaceParameterWith(sourceValue);
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