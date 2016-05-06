namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class OverwriteNullNestedAccessStrategy : DefaultNullNestedAccessStrategy
    {
        internal new static readonly INullNestedAccessStrategy Instance = new OverwriteNullNestedAccessStrategy();

        public override IMemberPopulation ProcessSingle(IMemberPopulation singleMemberPopulation)
        {
            var nestedAccessesCheck = GetNestedAccessesCheck(singleMemberPopulation);

            var valueOrDefault = Expression.Condition(
                nestedAccessesCheck,
                singleMemberPopulation.Value,
                Expression.Default(singleMemberPopulation.Value.Type));

            return singleMemberPopulation.WithValue(valueOrDefault);
        }
    }
}
