namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;

    internal class CopySourceEnumerablePopulationStrategy : EnumerablePopulationStrategyBase
    {
        public static readonly IEnumerablePopulationStrategy Instance = new CopySourceEnumerablePopulationStrategy();

        protected override Expression GetEnumerablePopulation(EnumerablePopulationBuilder builder)
            => MapSourceEnumerable(builder, b => b.AddVariableToTarget());

        public static EnumerablePopulationBuilder MapSourceEnumerable(
            EnumerablePopulationBuilder builder,
            params Func<EnumerablePopulationBuilder, Expression>[] nonNullTargetActionFactories)
        {
            builder.ProjectToTargetType().AssignValueToVariable();
            builder.IfTargetNotNull(nonNullTargetActionFactories);

            return builder;
        }
    }
}