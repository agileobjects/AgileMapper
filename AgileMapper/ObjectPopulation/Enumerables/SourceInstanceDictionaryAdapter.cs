namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;
    using Members;

    internal class SourceInstanceDictionaryAdapter : SourceEnumerableAdapterBase, ISourceEnumerableAdapter
    {
        private readonly DefaultSourceEnumerableAdapter _defaultAdapter;

        public SourceInstanceDictionaryAdapter(
            DictionarySourceMember sourceMember,
            EnumerablePopulationBuilder builder)
            : base(builder)
        {
            _defaultAdapter = new DefaultSourceEnumerableAdapter(builder);
            DictionaryVariables = new DictionaryEntryVariablePair(sourceMember, builder.MapperData);
        }

        public DictionaryEntryVariablePair DictionaryVariables { get; }

        public override Expression GetSourceValues()
        {
            var elementType = DictionaryVariables.SourceMember.EntryMember.ElementType;
            var emptyTarget = DictionaryVariables.SourceMember.ValueType.GetEmptyInstanceCreation(elementType);
            var returnLabel = Expression.Label(emptyTarget.Type, "Return");
            var returnEmpty = Expression.Return(returnLabel, emptyTarget);

            var ifKeyNotFoundShortCircuit = GetKeyNotFoundShortCircuit(returnEmpty);
            var isValueIsNullShortCircuit = GetNullValueEntryShortCircuit(returnEmpty);

            var sourceValueBlock = Expression.Block(
                new[] { DictionaryVariables.Key, DictionaryVariables.Value },
                ifKeyNotFoundShortCircuit,
                isValueIsNullShortCircuit,
                Expression.Label(returnLabel, DictionaryVariables.Value));

            return sourceValueBlock;
        }

        public Expression GetKeyNotFoundShortCircuit(Expression shortCircuitReturn)
            => DictionaryVariables.GetKeyNotFoundShortCircuit(shortCircuitReturn);

        private Expression GetNullValueEntryShortCircuit(Expression shortCircuitReturn)
        {
            var valueAssignment = DictionaryVariables.GetEntryValueAssignment();
            var valueIsNull = DictionaryVariables.Value.GetIsDefaultComparison();
            var ifValueNullShortCircuit = Expression.IfThen(valueIsNull, shortCircuitReturn);

            return Expression.Block(valueAssignment, ifValueNullShortCircuit);
        }

        public Expression GetEntryValueAccess() => DictionaryVariables.GetEntryValueAccess();

        public Expression GetSourceCountAccess() => _defaultAdapter.GetSourceCountAccess();

        public IPopulationLoopData GetPopulationLoopData() => _defaultAdapter.GetPopulationLoopData();
    }
}