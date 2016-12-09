namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;
    using NetStandardPolyfills;

    internal class DefaultSourceEnumerableAdapter : ISourceEnumerableAdapter
    {
        private readonly EnumerablePopulationBuilder _builder;

        public DefaultSourceEnumerableAdapter(EnumerablePopulationBuilder builder)
        {
            _builder = builder;
        }

        public Expression GetSourceValue() => _builder.MapperData.SourceObject;

        public Expression GetSourceValues() => _builder.SourceVariable;

        public Expression GetSourceCountAccess()
        {
            if (_builder.SourceTypeHelper.IsArray)
            {
                return Expression.Property(_builder.SourceVariable, "Length");
            }

            var countPropertyInfo = _builder
                .SourceTypeHelper
                .CollectionInterfaceType
                .GetPublicInstanceProperty("Count");

            return Expression.Property(_builder.SourceVariable, countPropertyInfo);
        }

        public IPopulationLoopData GetPopulationLoopData()
        {
            if (_builder.SourceTypeHelper.HasListInterface)
            {
                return new IndexedSourcePopulationLoopData(_builder);
            }

            return new EnumerableSourcePopulationLoopData(_builder);
        }
    }
}