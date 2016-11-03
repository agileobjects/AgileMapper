namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;

    internal abstract class EnumerablePopulationStrategyBase : IEnumerablePopulationStrategy
    {
        public virtual bool DiscardExistingValues => false;

        public Expression GetPopulation(IObjectMappingData enumerableMappingData)
        {
            if (enumerableMappingData.MapperData.SourceMember.IsEnumerable)
            {
                var builder = enumerableMappingData.MapperData.EnumerablePopulationBuilder;

                return GetEnumerablePopulation(builder, enumerableMappingData);
            }

            return Constants.EmptyExpression;
        }

        protected abstract Expression GetEnumerablePopulation(
            EnumerablePopulationBuilder builder,
            IObjectMappingData mappingData);
    }
}
