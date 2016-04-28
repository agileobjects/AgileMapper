namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class ComplexTypeMappingLambdaFactory<TSource, TTarget>
        : ObjectMappingLambdaFactoryBase<TSource, TTarget>
    {
        public static readonly ObjectMappingLambdaFactoryBase<TSource, TTarget> Instance =
            new ComplexTypeMappingLambdaFactory<TSource, TTarget>();

        protected override Expression GetObjectResolution(IObjectMappingContext omc)
        {
            var existingObjectOrCreate = Expression
                .Coalesce(omc.ExistingObject, omc.GetCreateCall());

            return existingObjectOrCreate;
        }

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingContext omc)
        {
            var memberPopulations = MemberPopulationFactory
               .Create(omc)
               .Where(p => p.IsSuccessful)
               .ToArray();

            var processedPopulations = omc
                .MappingContext
                .RuleSet
                .Process(memberPopulations)
                .Select(d => d.Population)
                .ToArray();

            return processedPopulations;
        }
    }
}