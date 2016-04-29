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
            var targetVariableValue = GetObjectResolution(omc);
            var targetVariableAssignment = Expression.Assign(omc.TargetVariable, targetVariableValue);
            var objectPopulation = GetObjectPopulation(targetVariableValue, omc);
            var returnValue = GetReturnValue(targetVariableValue, omc);

            var mappingBlock = Expression.Block(
                new[] { omc.TargetVariable },
                new[] { targetVariableAssignment }
                    .Concat(objectPopulation)
                    .Concat(returnValue));

            var mapperLambda = Expression
                .Lambda<MapperFunc<TSource, TTarget>>(mappingBlock, omc.Parameter);

            return mapperLambda;
        }

        protected abstract Expression GetObjectResolution(IObjectMappingContext omc);

        protected abstract IEnumerable<Expression> GetObjectPopulation(Expression targetVariableValue, IObjectMappingContext omc);

        protected abstract Expression GetReturnValue(Expression targetVariableValue, IObjectMappingContext omc);
    }
}