namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal class DefaultSourceEnumerableAdapter : SourceEnumerableAdapterBase, ISourceEnumerableAdapter
    {
        public DefaultSourceEnumerableAdapter(EnumerablePopulationBuilder builder)
            : base(builder)
        {
        }

        public Expression GetSourceCountAccess() => SourceTypeHelper.GetCountFor(SourceValue);

        public Expression GetMappingShortCircuitOrNull() => null;

        public IPopulationLoopData GetPopulationLoopData()
        {
            if (SourceTypeHelper.HasListInterface)
            {
                return new IndexedSourcePopulationLoopData(Builder);
            }

            return new EnumerableSourcePopulationLoopData(Builder);
        }
    }
}