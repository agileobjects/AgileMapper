namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;
    using NetStandardPolyfills;

    internal class DefaultSourceEnumerableAdapter : SourceEnumerableAdapterBase, ISourceEnumerableAdapter
    {
        public DefaultSourceEnumerableAdapter(EnumerablePopulationBuilder builder)
            : base(builder)
        {
        }

        public Expression GetSourceValues() => SourceValue;

        public Expression GetSourceCountAccess()
        {
            if (SourceTypeHelper.IsArray)
            {
                return Expression.Property(SourceValue, "Length");
            }

            var countPropertyInfo = SourceTypeHelper
                .CollectionInterfaceType
                .GetPublicInstanceProperty("Count");

            return Expression.Property(SourceValue, countPropertyInfo);
        }

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