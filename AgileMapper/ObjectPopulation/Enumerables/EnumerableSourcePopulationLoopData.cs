namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class EnumerableSourcePopulationLoopData : IPopulationLoopData
    {
        private static readonly MethodInfo _enumeratorMoveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");
        private static readonly MethodInfo _disposeMethod = typeof(IDisposable).GetMethod("Dispose");

        private readonly EnumerablePopulationBuilder _builder;

        private readonly ParameterExpression _enumerator;

        public EnumerableSourcePopulationLoopData(EnumerablePopulationBuilder builder)
        {
            _builder = builder;

            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(builder.SourceTypeHelper.ElementType);
            _enumerator = Expression.Variable(enumeratorType, "enumerator");

            LoopExitCheck = Expression.Not(Expression.Call(_enumerator, _enumeratorMoveNextMethod));
            SourceElement = Expression.Property(_enumerator, "Current");
        }

        public Expression LoopExitCheck { get; }

        public Expression SourceElement { get; }

        public Expression GetElementToAdd(IObjectMappingData enumerableMappingData)
            => _builder.GetElementConversion(SourceElement, enumerableMappingData);

        public Expression Adapt(LoopExpression loop) => GetLoopBlock(loop, exp => exp, exp => exp);

        public BlockExpression GetLoopBlock(
            Expression loop,
            Func<Expression, Expression> enumeratorValueFactory,
            Func<Expression, Expression> finallyClauseFactory)
        {
            var getEnumeratorMethod = _builder.SourceTypeHelper.EnumerableInterfaceType.GetMethod("GetEnumerator");
            var getEnumeratorCall = Expression.Call(_builder.SourceValue, getEnumeratorMethod);
            var enumeratorValue = enumeratorValueFactory.Invoke(getEnumeratorCall);
            var enumeratorAssignment = Expression.Assign(_enumerator, enumeratorValue);

            var finallyClause = finallyClauseFactory.Invoke(Expression.Call(_enumerator, _disposeMethod));

            return Expression.Block(
                new[] { _enumerator },
                enumeratorAssignment,
                Expression.TryFinally(loop, finallyClause));
        }
    }
}