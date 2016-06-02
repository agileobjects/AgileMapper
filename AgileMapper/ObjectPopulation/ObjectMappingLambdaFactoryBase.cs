namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal abstract class ObjectMappingLambdaFactoryBase<TSource, TTarget, TInstance>
    {
        public virtual Expression<MapperFunc<TSource, TTarget, TInstance>> Create(IObjectMappingContext omc)
        {
            var returnLabelTarget = Expression.Label(omc.ExistingObject.Type, "Return");
            var returnNull = Expression.Return(returnLabelTarget, Expression.Default(omc.ExistingObject.Type));

            if (IsNotConstructable(omc))
            {
                return Expression.Lambda<MapperFunc<TSource, TTarget, TInstance>>(GetNullMappingBlock(returnNull), omc.Parameter);
            }

            var shortCircuitReturns = GetShortCircuitReturns(returnNull, omc);
            var instanceVariableValue = GetObjectResolution(omc);
            var instanceVariableAssignment = Expression.Assign(omc.InstanceVariable, instanceVariableValue);
            var objectPopulation = GetObjectPopulation(instanceVariableValue, omc);
            var returnValue = GetReturnValue(instanceVariableValue, omc);
            var returnLabel = Expression.Label(returnLabelTarget, returnValue);

            var mappingBlock = Expression.Block(
                new[] { omc.InstanceVariable },
                shortCircuitReturns
                    .Concat(instanceVariableAssignment)
                    .Concat(objectPopulation)
                    .Concat(returnLabel));

            var mapperLambda = Expression
                .Lambda<MapperFunc<TSource, TTarget, TInstance>>(mappingBlock, omc.Parameter);

            return mapperLambda;
        }

        private static Expression GetNullMappingBlock(GotoExpression returnNull)
        {
            return Expression.Block(
                ReadableExpression.Comment("Unable to construct object of Type " + returnNull.Value.Type.GetFriendlyName()),
                returnNull.Value);
        }

        protected abstract bool IsNotConstructable(IObjectMappingContext omc);

        protected abstract IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingContext omc);

        protected abstract Expression GetObjectResolution(IObjectMappingContext omc);

        protected abstract IEnumerable<Expression> GetObjectPopulation(Expression instanceVariableValue, IObjectMappingContext omc);

        protected abstract Expression GetReturnValue(Expression instanceVariableValue, IObjectMappingContext omc);
    }
}