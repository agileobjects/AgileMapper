namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Collections;
#if !NET_STANDARD
    using System.Diagnostics.CodeAnalysis;
#endif
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using NetStandardPolyfills;

    internal class SourceObjectDictionaryAdapter : SourceEnumerableAdapterBase, ISourceEnumerableAdapter
    {
        private readonly SourceInstanceDictionaryAdapter _instanceDictionaryAdapter;
        private readonly Expression _emptyTarget;

        public SourceObjectDictionaryAdapter(
            DictionarySourceMember sourceMember,
            EnumerablePopulationBuilder builder)
            : base(builder)
        {
            _instanceDictionaryAdapter = new SourceInstanceDictionaryAdapter(sourceMember, builder);

            var targetEnumerableType = TargetTypeHelper.EnumerableInterfaceType;
            _emptyTarget = targetEnumerableType.GetEmptyInstanceCreation(TargetTypeHelper.ElementType);
        }

        public override Expression GetSourceValue()
        {
            var returnLabel = Expression.Label(_emptyTarget.Type, "Return");
            var returnEmpty = Expression.Return(returnLabel, _emptyTarget);

            var ifKeyNotFoundShortCircuit = _instanceDictionaryAdapter.GetKeyNotFoundShortCircuit(returnEmpty);

            var enumerableAssignment = GetUntypedEnumerableAssignment(out var untypedEnumerableVariable);

            var enumerableIsNull = untypedEnumerableVariable.GetIsDefaultComparison();
            var ifNotEnumerableReturnEmpty = Expression.IfThen(enumerableIsNull, returnEmpty);

            var typedEnumerableAssignment =
                GetTypedEnumerableAssignment(untypedEnumerableVariable, out var typedEnumerableVariable);

            var enumerableIsTyped = typedEnumerableVariable.GetIsNotDefaultComparison();
            var returnTypedEnumerable = Expression.Return(returnLabel, typedEnumerableVariable);
            var ifTypedEnumerableReturn = Expression.IfThen(enumerableIsTyped, returnTypedEnumerable);

            var returnProjectionResult = GetEntryValueProjection(untypedEnumerableVariable, returnLabel);

            var sourceValueBlock = Expression.Block(
                new[]
                {
                    _instanceDictionaryAdapter.DictionaryVariables.Key,
                    untypedEnumerableVariable,
                    typedEnumerableVariable
                },
                ifKeyNotFoundShortCircuit,
                enumerableAssignment,
                ifNotEnumerableReturnEmpty,
                typedEnumerableAssignment,
                ifTypedEnumerableReturn,
                returnProjectionResult);

            return sourceValueBlock;
        }

        private Expression GetUntypedEnumerableAssignment(out ParameterExpression enumerableVariable)
        {
            enumerableVariable = Expression.Variable(typeof(IEnumerable), "sourceEnumerable");
            var sourceValue = _instanceDictionaryAdapter.GetEntryValueAccess();
            var valueAsEnumerable = Expression.TypeAs(sourceValue, typeof(IEnumerable));
            var enumerableAssignment = enumerableVariable.AssignTo(valueAsEnumerable);

            return enumerableAssignment;
        }

        private Expression GetTypedEnumerableAssignment(
            Expression untypedEnumerableVariable,
            out ParameterExpression typedEnumerableVariable)
        {
            var targetEnumerableType = _emptyTarget.Type;
            var enumerableAsTyped = Expression.TypeAs(untypedEnumerableVariable, targetEnumerableType);
            var typedEnumerableVariableName = targetEnumerableType.GetVariableNameInCamelCase();
            typedEnumerableVariable = Expression.Variable(targetEnumerableType, typedEnumerableVariableName);
            var typedEnumerableAssignment = typedEnumerableVariable.AssignTo(enumerableAsTyped);

            return typedEnumerableAssignment;
        }

        private Expression GetEntryValueProjection(
            Expression untypedEnumerableVariable,
            LabelTarget returnLabel)
        {
            var linqCastMethod = typeof(Enumerable).GetPublicStaticMethod("Cast");
            var typedCastMethod = linqCastMethod.MakeGenericMethod(typeof(object));
            var linqCastCall = Expression.Call(null, typedCastMethod, untypedEnumerableVariable);

            var sourceItemsProjection = Builder.Context.ElementTypesAreSimple
                ? Builder.GetSourceItemsProjection(linqCastCall, GetSourceElementConversion)
                : Builder.GetSourceItemsProjection(linqCastCall, GetSourceElementMapping);

            var returnProjectionResult = Expression.Label(returnLabel, sourceItemsProjection);

            return returnProjectionResult;
        }

        private Expression GetSourceElementConversion(Expression sourceParameter)
            => Builder.GetSimpleElementConversion(sourceParameter);

        private Expression GetSourceElementMapping(Expression sourceParameter, Expression counter)
            => Builder.MapperData.GetMapCall(sourceParameter);

        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public Expression GetSourceValues()
        {
            // This is called to provide a value for a List.AddRange() call,
            // which requires the source and target elements to be simple and
            // of the same type. This class is for Dictionary<string, object>,
            // so this is never called:
            return null;
        }

        public Expression GetSourceCountAccess() => _instanceDictionaryAdapter.GetSourceCountAccess();

        public override bool UseReadOnlyTargetWrapper
            => base.UseReadOnlyTargetWrapper && Builder.Context.ElementTypesAreSimple;

        public IPopulationLoopData GetPopulationLoopData()
        {
            return new SourceObjectDictionaryPopulationLoopData(
                _emptyTarget,
                _instanceDictionaryAdapter.DictionaryVariables,
                Builder);
        }
    }
}