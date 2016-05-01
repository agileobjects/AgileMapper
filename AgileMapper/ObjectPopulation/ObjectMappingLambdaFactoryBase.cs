namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal abstract class ObjectMappingLambdaFactoryBase<TSource, TTarget>
    {
        public Expression<MapperFunc<TSource, TTarget>> Create(IObjectMappingContext omc)
        {
            var returnLabelTarget = Expression.Label(omc.ExistingObject.Type, "Return");
            var returnNull = Expression.Return(returnLabelTarget, Expression.Default(omc.ExistingObject.Type));

            var shortCircuitReturns = GetShortCircuitReturns(returnNull, omc);
            var targetVariableValue = GetObjectResolution(omc);
            var targetVariableAssignment = Expression.Assign(omc.TargetVariable, targetVariableValue);
            var objectPopulation = GetObjectPopulation(targetVariableValue, omc);
            var returnValue = GetReturnValue(targetVariableValue, omc);
            var returnLabel = Expression.Label(returnLabelTarget, returnValue);

            var mappingBlock = Expression.Block(
                new[] { omc.TargetVariable },
                shortCircuitReturns
                    .Concat(targetVariableAssignment)
                    .Concat(objectPopulation)
                    .Concat(returnLabel));

            var mapperLambda = Expression
                .Lambda<MapperFunc<TSource, TTarget>>(mappingBlock, omc.Parameter);

            return mapperLambda;
        }

        protected abstract IEnumerable<Expression> GetShortCircuitReturns(Expression returnNull, IObjectMappingContext omc);

        protected abstract Expression GetObjectResolution(IObjectMappingContext omc);

        protected abstract IEnumerable<Expression> GetObjectPopulation(Expression targetVariableValue, IObjectMappingContext omc);

        protected abstract Expression GetReturnValue(Expression targetVariableValue, IObjectMappingContext omc);
    }
}