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

        public override Expression GetSourceValues()
            => Expression.Property(base.GetSourceValues(), "Values");

        public Expression GetSourceCountAccess()
            => Expression.Property(SourceValue, "Count");

        public override bool UseReadOnlyTargetWrapper
            => base.UseReadOnlyTargetWrapper && Builder.Context.ElementTypesAreSimple;

        public IPopulationLoopData GetPopulationLoopData()
        {
            if (Builder.ElementTypesAreSimple)
            {
                return new EnumerableSourcePopulationLoopData(Builder);
            }

            return new SourceElementsDictionaryPopulationLoopData(_sourceMember, Builder);
        }
    }
}