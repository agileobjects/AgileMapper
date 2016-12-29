namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
#if !NET_STANDARD
    using System.Diagnostics.CodeAnalysis;
#endif
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

        public override Expression GetSourceValue()
        {
            var emptyTarget = DictionaryVariables.SourceMember.ValueType.GetEmptyInstanceCreation();
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

        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
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