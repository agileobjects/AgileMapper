namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;

    internal class MergeEnumerablePopulationStrategy : EnumerablePopulationStrategyBase
    {
        public static readonly IEnumerablePopulationStrategy Instance = new MergeEnumerablePopulationStrategy();

        protected override Expression GetEnumerablePopulation(EnumerablePopulationBuilder builder)
        {
            if (builder.TypesAreIdentifiable)
            {
                return MergeEnumerables(
                    builder,
                    b => b.MapIntersection(),
                    b => b.AddVariableToTarget());
            }

            builder
                .ProjectToTargetType()
                .ExcludeTarget()
                .AssignValueToVariable();

            builder.IfTargetNotNull(b => b.AddVariableToTarget());

            return builder;
        }

        public static EnumerablePopulationBuilder MergeEnumerables(
            EnumerablePopulationBuilder builder,
            params Func<EnumerablePopulationBuilder, Expression>[] nonNullTargetActionFactories)
        {
            builder.CreateCollectionData();

            CopySourceEnumerablePopulationStrategy.MapSourceEnumerable(builder, nonNullTargetActionFactories);

            return builder;
        }
    }
}