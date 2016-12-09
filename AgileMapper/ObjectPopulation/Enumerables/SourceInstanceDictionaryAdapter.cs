namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;
    using Members;

    internal class SourceInstanceDictionaryAdapter : ISourceEnumerableAdapter
    {
        private readonly DefaultSourceEnumerableAdapter _defaultAdapter;
        private readonly DictionarySourceMember _sourceMember;
        private readonly EnumerablePopulationBuilder _builder;

        public SourceInstanceDictionaryAdapter(
            DictionarySourceMember sourceMember,
            EnumerablePopulationBuilder builder)
        {
            _defaultAdapter = new DefaultSourceEnumerableAdapter(builder);
            _sourceMember = sourceMember;
            _builder = builder;

            DictionaryVariables = new DictionaryEntryVariablePair(sourceMember, builder.MapperData);
        }

        public DictionaryEntryVariablePair DictionaryVariables { get; }

        public Expression GetSourceValue()
        {
            var emptyTarget = _sourceMember.EntryType.GetEmptyInstanceCreation();
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
        {
            var sourceValueKeyAssignment = DictionaryVariables.GetMatchingKeyAssignment(_builder.MapperData);
            var keyNotFound = DictionaryVariables.Key.GetIsDefaultComparison();
            var ifKeyNotFoundShortCircuit = Expression.IfThen(keyNotFound, shortCircuitReturn);

            return Expression.Block(sourceValueKeyAssignment, ifKeyNotFoundShortCircuit);
        }

        private Expression GetNullValueEntryShortCircuit(Expression shortCircuitReturn)
        {
            var sourceValue = DictionaryVariables.GetEntryValueAccess(_builder.MapperData);
            var valueAssignment = Expression.Assign(DictionaryVariables.Value, sourceValue);
            var valueIsNull = DictionaryVariables.Value.GetIsDefaultComparison();
            var ifValueNullShortCircuit = Expression.IfThen(valueIsNull, shortCircuitReturn);

            return Expression.Block(valueAssignment, ifValueNullShortCircuit);
        }

        public Expression GetEntryValueAccess()
            => DictionaryVariables.GetEntryValueAccess(_builder.MapperData);

        public Expression GetSourceValues()
        {
            // This is called to provide a value for a List.AddRange() call,
            // which requires the source and target elements to be simple and
            // of the same type. This class is for Dictionary<string, IEnumerable<T>>,
            // so this is never called:
            return null;
        }

        public Expression GetSourceCountAccess() => _defaultAdapter.GetSourceCountAccess();

        public IPopulationLoopData GetPopulationLoopData() => _defaultAdapter.GetPopulationLoopData();
    }
}