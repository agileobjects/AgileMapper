namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Collections;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using NetStandardPolyfills;

    internal class SourceObjectDictionaryAdapter : ISourceEnumerableAdapter
    {
        private readonly EnumerablePopulationBuilder _builder;
        private readonly SourceInstanceDictionaryAdapter _instanceDictionaryAdapter;
        private readonly Expression _emptyTarget;

        public SourceObjectDictionaryAdapter(
            DictionarySourceMember sourceMember,
            EnumerablePopulationBuilder builder)
        {
            _builder = builder;
            _instanceDictionaryAdapter = new SourceInstanceDictionaryAdapter(sourceMember, builder);

            var targetEnumerableType = builder.TargetTypeHelper.EnumerableInterfaceType;
            _emptyTarget = targetEnumerableType.GetEmptyInstanceCreation();
        }

        public Expression GetSourceValue()
        {
            var returnLabel = Expression.Label(_emptyTarget.Type, "Return");
            var returnEmpty = Expression.Return(returnLabel, _emptyTarget);

            var ifKeyNotFoundShortCircuit = _instanceDictionaryAdapter.GetKeyNotFoundShortCircuit(returnEmpty);

            ParameterExpression untypedEnumerableVariable;
            var enumerableAssignment = GetUntypedEnumerableAssignment(out untypedEnumerableVariable);

            var enumerableIsNull = untypedEnumerableVariable.GetIsDefaultComparison();
            var ifNotEnumerableReturnEmpty = Expression.IfThen(enumerableIsNull, returnEmpty);

            ParameterExpression typedEnumerableVariable;
            var typedEnumerableAssignment =
                GetTypedEnumerableAssignment(untypedEnumerableVariable, out typedEnumerableVariable);

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
            var enumerableAssignment = Expression.Assign(enumerableVariable, valueAsEnumerable);

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
            var typedEnumerableAssignment = Expression.Assign(typedEnumerableVariable, enumerableAsTyped);

            return typedEnumerableAssignment;
        }

        private Expression GetEntryValueProjection(
            Expression untypedEnumerableVariable,
            LabelTarget returnLabel)
        {
            var linqCastMethod = typeof(Enumerable).GetPublicStaticMethod("Cast");
            var typedCastMethod = linqCastMethod.MakeGenericMethod(typeof(object));
            var linqCastCall = Expression.Call(null, typedCastMethod, untypedEnumerableVariable);

            var sourceItemsProjection = _builder.Context.ElementTypesAreSimple
                ? _builder.GetSourceItemsProjection(linqCastCall, GetSourceElementConversion)
                : _builder.GetSourceItemsProjection(linqCastCall, GetSourceElementMapping);

            var returnProjectionResult = Expression.Label(returnLabel, sourceItemsProjection);

            return returnProjectionResult;
        }

        private Expression GetSourceElementConversion(Expression sourceParameter)
            => _builder.GetSimpleElementConversion(sourceParameter);

        private Expression GetSourceElementMapping(Expression sourceParameter, Expression counter)
            => _builder.MapperData.GetMapCall(sourceParameter);

        public Expression GetSourceValues()
        {
            // This is called to provide a value for a List.AddRange() call,
            // which requires the source and target elements to be simple and
            // of the same type. This class is for Dictionary<string, object>,
            // so this is never called:
            return null;
        }

        public Expression GetSourceCountAccess() => _instanceDictionaryAdapter.GetSourceCountAccess();

        public IPopulationLoopData GetPopulationLoopData()
        {
            return new SourceObjectDictionaryPopulationLoopData(
                _emptyTarget,
                _instanceDictionaryAdapter.DictionaryVariables,
                _builder);
        }
    }
}