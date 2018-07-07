namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class EnumerableSourcePopulationLoopData : IPopulationLoopData
    {
        private readonly Expression _enumerableSubject;
        private readonly MethodInfo _getEnumeratorMethod;
        private readonly ParameterExpression _enumerator;

        public EnumerableSourcePopulationLoopData(EnumerablePopulationBuilder builder)
            : this(builder, builder.SourceTypeHelper.ElementType, builder.SourceValue)
        {
        }

        public EnumerableSourcePopulationLoopData(
            EnumerablePopulationBuilder builder,
            Type elementType,
            Expression enumerableSubject)
        {
            Builder = builder;
            _enumerableSubject = enumerableSubject;

            _getEnumeratorMethod = typeof(IEnumerable<>).MakeGenericType(elementType).GetPublicInstanceMethod("GetEnumerator");
            _enumerator = Expression.Variable(_getEnumeratorMethod.ReturnType, "enumerator");

            ContinueLoopTarget = Expression.Label(typeof(void), "Continue");
            LoopExitCheck = Expression.Not(Expression.Call(_enumerator, typeof(IEnumerator).GetPublicInstanceMethod("MoveNext")));
            SourceElement = Expression.Property(_enumerator, "Current");
        }

        public EnumerablePopulationBuilder Builder { get; }

        public bool NeedsContinueTarget { get; set; }

        public LabelTarget ContinueLoopTarget { get; }

        public Expression LoopExitCheck { get; }

        public Expression SourceElement { get; }

        public virtual Expression GetSourceElementValue() => SourceElement;

        public Expression GetElementMapping(IObjectMappingData enumerableMappingData)
            => Builder.GetElementConversion(GetSourceElementValue(), enumerableMappingData);

        public Expression Adapt(LoopExpression loop) => GetLoopBlock(loop);

        public BlockExpression GetLoopBlock(
            Expression loop,
            Func<Expression, Expression> enumeratorValueFactory = null,
            Func<Expression, Expression> finallyClauseFactory = null)
        {
            Expression enumeratorValue = Expression.Call(_enumerableSubject, _getEnumeratorMethod);

            if (enumeratorValueFactory != null)
            {
                enumeratorValue = enumeratorValueFactory.Invoke(enumeratorValue);
            }

            var enumeratorAssignment = _enumerator.AssignTo(enumeratorValue);

            Expression finallyClause = Expression.Call(
                _enumerator,
                typeof(IDisposable).GetPublicInstanceMethod("Dispose"));

            if (finallyClauseFactory != null)
            {
                finallyClause = finallyClauseFactory.Invoke(finallyClause);
            }

            return Expression.Block(
                new[] { _enumerator },
                enumeratorAssignment,
                Expression.TryFinally(loop, finallyClause));
        }
    }
}