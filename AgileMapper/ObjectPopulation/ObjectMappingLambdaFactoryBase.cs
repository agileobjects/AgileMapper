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
            var targetVariableAssignment = Expression.Assign(omc.TargetVariable, GetObjectResolution(omc));
            var objectPopulation = GetObjectPopulation(omc);

            var mappingBlock = Expression.Block(
                new[] { omc.TargetVariable },
                new[] { targetVariableAssignment }
                    .Concat(objectPopulation)
                    .Concat(omc.TargetVariable));

            var mapperLambda = Expression
                .Lambda<MapperFunc<TSource, TTarget>>(mappingBlock, omc.Parameter);

            return mapperLambda;
        }

        protected abstract Expression GetObjectResolution(IObjectMappingContext omc);

        protected abstract IEnumerable<Expression> GetObjectPopulation(IObjectMappingContext omc);
    }
}