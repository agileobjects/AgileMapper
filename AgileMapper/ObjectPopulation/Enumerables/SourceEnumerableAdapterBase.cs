namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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

        public virtual Expression GetSourceValues() => Builder.MapperData.SourceObject;

        public virtual bool UseReadOnlyTargetWrapper =>
            TargetTypeHelper.IsReadOnly && !SourceTypeHelper.IsEnumerableInterface;
    }
}