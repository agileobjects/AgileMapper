namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class EnumerableMappingLambdaFactory<TSource, TTarget>
        : ObjectMappingLambdaFactoryBase<TSource, TTarget>
    {
        public static readonly ObjectMappingLambdaFactoryBase<TSource, TTarget> Instance =
            new EnumerableMappingLambdaFactory<TSource, TTarget>();

        protected override bool IsNotConstructable(IObjectMappingContext omc) => false;

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingContext omc)
            => Enumerable.Empty<Expression>();

        protected override Expression GetObjectResolution(IObjectMappingContext omc)
            => EnumerableTypes.GetEnumerableVariableValue(omc.TargetObject, omc.TargetMember.Type);

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingContext omc)
        {
            yield return omc.MappingContext.RuleSet.EnumerablePopulationStrategy.GetPopulation(omc);
        }

        protected override Expression GetReturnValue(IObjectMappingContext omc)
        {
            if (omc.TargetMember.Type.IsAssignableFrom(omc.InstanceVariable.Type))
            {
                return omc.InstanceVariable;
            }

            return omc.InstanceVariable.WithToArrayCall();
        }
    }
}