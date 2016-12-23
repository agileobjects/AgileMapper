namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;
    using Members;

    internal class SourceElementsDictionaryAdapter : SourceEnumerableAdapterBase, ISourceEnumerableAdapter
    {
        private readonly DictionarySourceMember _sourceMember;

        public SourceElementsDictionaryAdapter(
            DictionarySourceMember sourceMember,
            EnumerablePopulationBuilder builder)
            : base(builder)
        {
            _sourceMember = sourceMember;
        }

        public Expression GetSourceValues()
            => Expression.Property(GetSourceValue(), "Values");

        public Expression GetSourceCountAccess()
            => Expression.Property(SourceValue, "Count");

        public override bool UseReadOnlyTargetWrapper
            => base.UseReadOnlyTargetWrapper && Builder.Context.ElementTypesAreSimple;

        public IPopulationLoopData GetPopulationLoopData()
            => new SourceElementsDictionaryPopulationLoopData(_sourceMember, Builder);
    }
}