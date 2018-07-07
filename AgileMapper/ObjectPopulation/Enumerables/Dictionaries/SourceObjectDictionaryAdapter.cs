namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Dictionaries
{
    using System.Collections;
    using System.Linq;
    using Extensions.Internal;
    using Members.Dictionaries;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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
            _emptyTarget = TargetTypeHelper.GetEmptyInstanceCreation(TargetTypeHelper.EnumerableInterfaceType);
        }

        public override Expression GetSourceValues()
        {
            var returnLabel = Expression.Label(_emptyTarget.Type, "Return");
            var returnEmpty = Expression.Return(returnLabel, _emptyTarget);

            var ifKeyNotFoundShortCircuit = _instanceDictionaryAdapter.GetKeyNotFoundShortCircuit(returnEmpty);

            var enumerableAssignment = GetUntypedEnumerableAssignment(out var untypedEnumerableVariable);

            var enumerableIsNull = untypedEnumerableVariable.GetIsDefaultComparison();
            var ifNotEnumerableReturnEmpty = Expression.IfThen(enumerableIsNull, returnEmpty);

            var returnProjectionResult = GetEntryValueProjection(untypedEnumerableVariable, returnLabel);

            var sourceValueBlock = Expression.Block(
                new[]
                {
                    _instanceDictionaryAdapter.DictionaryVariables.Key,
                    untypedEnumerableVariable
                },
                ifKeyNotFoundShortCircuit,
                enumerableAssignment,
                ifNotEnumerableReturnEmpty,
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

        private Expression GetEntryValueProjection(
            Expression untypedEnumerableVariable,
            LabelTarget returnLabel)
        {
            Expression sourceItemsProjection;

            if (Builder.Context.ElementTypesAreSimple)
            {
                var linqCastMethod = typeof(Enumerable).GetPublicStaticMethod("Cast");
                var typedCastMethod = linqCastMethod.MakeGenericMethod(typeof(object));
                var linqCastCall = Expression.Call(null, typedCastMethod, untypedEnumerableVariable);
                sourceItemsProjection = Builder.GetSourceItemsProjection(linqCastCall, GetSourceElementConversion);
            }
            else
            {
                sourceItemsProjection = Builder.MapperData.Parent.GetMapCall(
                    untypedEnumerableVariable,
                    Builder.MapperData.TargetMember,
                    dataSourceIndex: 0);
            }

            var returnProjectionResult = Expression.Label(returnLabel, sourceItemsProjection);

            return returnProjectionResult;
        }

        private Expression GetSourceElementConversion(Expression sourceParameter)
            => Builder.GetSimpleElementConversion(sourceParameter);

        public Expression GetSourceCountAccess() => _instanceDictionaryAdapter.GetSourceCountAccess();

        public override bool UseReadOnlyTargetWrapper
            => base.UseReadOnlyTargetWrapper && Builder.Context.ElementTypesAreSimple;

        public Expression GetMappingShortCircuitOrNull()
        {
            if (Builder.ElementTypesAreSimple)
            {
                return null;
            }

            var sourceEnumerableFoundTest = SourceObjectDictionaryPopulationLoopData
                .GetSourceEnumerableFoundTest(_emptyTarget, Builder);

            var projectionAsTargetType = Expression.TypeAs(Builder.SourceValue, Builder.MapperData.TargetType);
            var allowEnumerableAssignment = Builder.MapperData.RuleSet.Settings.AllowEnumerableAssignment;
            var convertedProjection = TargetTypeHelper.GetEnumerableConversion(Builder.SourceValue, allowEnumerableAssignment);
            var projectionResult = Expression.Coalesce(projectionAsTargetType, convertedProjection);
            var returnConvertedProjection = Expression.Return(Builder.MapperData.ReturnLabelTarget, projectionResult);
            var ifProjectedReturn = Expression.IfThen(sourceEnumerableFoundTest, returnConvertedProjection);

            return ifProjectedReturn;
        }

        public IPopulationLoopData GetPopulationLoopData()
        {
            return new SourceObjectDictionaryPopulationLoopData(
                _emptyTarget,
                _instanceDictionaryAdapter.DictionaryVariables,
                Builder);
        }
    }
}