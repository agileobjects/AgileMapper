namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class EnumerableMappingLambdaFactory<TSource, TTarget>
        : ObjectMappingLambdaFactoryBase<TSource, TTarget>
    {
        public static readonly ObjectMappingLambdaFactoryBase<TSource, TTarget> Instance =
            new EnumerableMappingLambdaFactory<TSource, TTarget>();

        protected override bool IsNotConstructable(IObjectMappingContext omc) => false;

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingContext omc)
            => Enumerable.Empty<Expression>();

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingContext omc)
        {
            yield return omc.MappingContext.RuleSet.EnumerablePopulationStrategy.GetPopulation(omc);
        }

        protected override Expression GetReturnValue(IObjectMappingContext omc)
        {
            return omc.SourceType.IsEnumerable()
                ? omc.EnumerablePopulationBuilder.GetReturnValue()
                : omc.EnumerablePopulationBuilder.ExistingOrNewEmptyInstance();
        }
    }
}