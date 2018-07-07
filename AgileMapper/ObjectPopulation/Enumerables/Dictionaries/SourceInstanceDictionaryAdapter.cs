namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Dictionaries
{
    using DataSources;
    using Extensions.Internal;
    using Members.Dictionaries;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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
            var isValueIsNullShortCircuit = GetNullValueEntryShortCircuitIfAppropriate(returnEmpty);

            var sourceValueBlock = Expression.Block(
                new[] { DictionaryVariables.Key, DictionaryVariables.Value },
                ifKeyNotFoundShortCircuit,
                isValueIsNullShortCircuit,
                Expression.Label(returnLabel, DictionaryVariables.Value));

            return sourceValueBlock;
        }

        public Expression GetKeyNotFoundShortCircuit(Expression shortCircuitReturn)
            => DictionaryVariables.GetKeyNotFoundShortCircuit(shortCircuitReturn);

        private Expression GetNullValueEntryShortCircuitIfAppropriate(Expression shortCircuitReturn)
        {
            var valueAssignment = DictionaryVariables.GetEntryValueAssignment();

            if (shortCircuitReturn.Type.CannotBeNull())
            {
                return valueAssignment;
            }

            var valueIsNull = DictionaryVariables.Value.GetIsDefaultComparison();
            var ifValueNullShortCircuit = Expression.IfThen(valueIsNull, shortCircuitReturn);

            return Expression.Block(valueAssignment, ifValueNullShortCircuit);
        }

        public Expression GetEntryValueAccess() => DictionaryVariables.GetEntryValueAccess();

        public Expression GetSourceCountAccess() => _defaultAdapter.GetSourceCountAccess();

        public Expression GetMappingShortCircuitOrNull() => null;

        public IPopulationLoopData GetPopulationLoopData() => _defaultAdapter.GetPopulationLoopData();
    }
}