namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;
    using Members;

    internal class SourceElementsDictionaryAdapter : ISourceEnumerableAdapter
    {
        private readonly DictionarySourceMember _sourceMember;
        private readonly EnumerablePopulationBuilder _builder;

        public SourceElementsDictionaryAdapter(
            DictionarySourceMember sourceMember,
            EnumerablePopulationBuilder builder)
        {
            _sourceMember = sourceMember;
            _builder = builder;
        }

        public Expression GetSourceValue() => _builder.MapperData.SourceObject;

        public Expression GetSourceValues()
            => Expression.Property(_builder.MapperData.SourceObject, "Values");

        public Expression GetSourceCountAccess()
            => Expression.Property(_builder.SourceValue, "Count");

        public IPopulationLoopData GetPopulationLoopData()
            => new SourceElementsDictionaryPopulationLoopData(_sourceMember, _builder);
    }
}