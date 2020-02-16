namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Dictionaries
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using EnumerableExtensions;
    using Members.Dictionaries;

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

        public override Expression GetElementKey() => DictionaryVariables.Key;

        public override Expression GetSourceValues()
        {
            var elementType = DictionaryVariables.SourceMember.EntryMember.ElementType;
            var emptyTarget = DictionaryVariables.SourceMember.ValueType.GetEmptyInstanceCreation(elementType);
            var returnLabel = Expression.Label(emptyTarget.Type, "Return");
            var returnEmpty = Expression.Return(returnLabel, emptyTarget);

            var ifKeyNotFoundShortCircuit = GetKeyNotFoundShortCircuit(returnEmpty);
            var isValueIsNullShortCircuit = DictionaryVariables.GetEntryValueAssignment();

            var sourceValueBlock = Expression.Block(
                new[] { DictionaryVariables.Key, DictionaryVariables.Value },
                ifKeyNotFoundShortCircuit,
                isValueIsNullShortCircuit,
                Expression.Label(returnLabel, DictionaryVariables.Value));

            return sourceValueBlock;
        }

        public Expression GetKeyNotFoundShortCircuit(Expression shortCircuitReturn)
            => DictionaryVariables.GetKeyNotFoundShortCircuit(shortCircuitReturn);

        public Expression GetEntryValueAccess() => DictionaryVariables.GetEntryValueAccess();

        public Expression GetSourceCountAccess() => _defaultAdapter.GetSourceCountAccess();

        public Expression GetMappingShortCircuitOrNull() => null;

        public IPopulationLoopData GetPopulationLoopData() => _defaultAdapter.GetPopulationLoopData();
    }
}