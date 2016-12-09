namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal class SourceElementsDictionaryAdapter : ISourceEnumerableAdapter
    {
        private readonly EnumerablePopulationBuilder _builder;

        public SourceElementsDictionaryAdapter(EnumerablePopulationBuilder builder)
        {
            _builder = builder;
        }

        public Expression GetSourceValue() => _builder.MapperData.SourceObject;

        public Expression GetSourceValues()
            => Expression.Property(_builder.MapperData.SourceObject, "Values");

        public Expression GetSourceCountAccess()
            => Expression.Property(_builder.SourceVariable, "Count");

        public IPopulationLoopData GetPopulationLoopData()
            => new SourceElementsDictionaryPopulationLoopData(_builder);
    }
}