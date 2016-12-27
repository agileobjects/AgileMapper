namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;

    internal abstract class SourceEnumerableAdapterBase
    {
        protected SourceEnumerableAdapterBase(EnumerablePopulationBuilder builder)
        {
            Builder = builder;
        }

        protected EnumerablePopulationBuilder Builder { get; }

        protected EnumerableTypeHelper SourceTypeHelper => Builder.SourceTypeHelper;

        protected Expression SourceValue => Builder.SourceValue;

        protected EnumerableTypeHelper TargetTypeHelper => Builder.TargetTypeHelper;

        public virtual Expression GetSourceValue() => Builder.MapperData.SourceObject;

        public virtual bool UseReadOnlyTargetWrapper =>
            TargetTypeHelper.IsArray && !SourceTypeHelper.IsEnumerableInterface;
    }
}