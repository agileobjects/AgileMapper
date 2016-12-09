namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;
    using Extensions;

    internal class IndexedSourcePopulationLoopData : IPopulationLoopData
    {
        private readonly EnumerablePopulationBuilder _builder;
        private readonly Expression _indexedSourceAccess;
        private readonly bool _useDirectValueAccess;
        private readonly Expression _sourceElement;

        public IndexedSourcePopulationLoopData(EnumerablePopulationBuilder builder)
        {
            _builder = builder;
            LoopExitCheck = Expression.Equal(builder.Counter, builder.GetSourceCountAccess());

            _indexedSourceAccess = builder.GetSourceIndexAccess();

            _useDirectValueAccess =
                builder.ElementTypesAreSimple ||
                builder.SourceTypeHelper.ElementType.RuntimeTypeNeeded() ||
                builder.TargetTypeHelper.ElementType.RuntimeTypeNeeded();

            _sourceElement = _useDirectValueAccess
                ? _indexedSourceAccess
                : builder.Context.GetSourceParameterFor(builder.SourceTypeHelper.ElementType);
        }

        public Expression LoopExitCheck { get; }

        public Expression GetElementToAdd(IObjectMappingData enumerableMappingData)
            => _builder.GetElementConversion(_sourceElement, enumerableMappingData);

        public Expression Adapt(LoopExpression loop)
        {
            if (_useDirectValueAccess)
            {
                return loop;
            }

            return loop.InsertAssignment(
                Constants.AfterLoopExitCheck,
                (ParameterExpression)_sourceElement,
                _indexedSourceAccess);
        }
    }
}