namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal abstract class ObjectMappingLambdaFactoryBase<TSource, TTarget, TInstance>
    {
        public Expression<MapperFunc<TSource, TTarget, TInstance>> Create(IObjectMappingContext omc)
        {
            var returnLabelTarget = Expression.Label(omc.ExistingObject.Type, "Return");
            var returnNull = Expression.Return(returnLabelTarget, Expression.Default(omc.ExistingObject.Type));

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

        protected abstract IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingContext omc);

        protected abstract Expression GetObjectResolution(IObjectMappingContext omc);

        protected abstract IEnumerable<Expression> GetObjectPopulation(Expression instanceVariableValue, IObjectMappingContext omc);

        protected abstract Expression GetReturnValue(Expression instanceVariableValue, IObjectMappingContext omc);
    }
}