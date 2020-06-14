namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes.ShortCircuits
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using Extensions;
    using Extensions.Internal;
    using Members;

    internal static class SourceDictionaryShortCircuitFactory
    {
        public static Expression GetShortCircuitOrNull(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (!IsFor(mapperData))
            {
                return null;
            }

            var dictionaryVariables = new DictionaryEntryVariablePair(mapperData);
            var fallbackValue = mapperData.GetTargetFallbackValue();

            var noMatchingKeys = dictionaryVariables.GetNoKeysWithMatchingStartQuery();
            var returnFallback = mapperData.GetReturnExpression(fallbackValue);
            var ifNoMatchingKeysShortCircuit = Expression.IfThen(noMatchingKeys, returnFallback);

            var foundValueNonNull = dictionaryVariables.Value.GetIsNotDefaultComparison();

            var entryExistsTest = GetEntryExistsTest(dictionaryVariables);

            var mapValueCall = GetMapValueCall(dictionaryVariables.Value, mapperData);

            var valueMappingOrFallback = Expression.Condition(foundValueNonNull, mapValueCall, fallbackValue);
            var returnMapValueResult = mapperData.GetReturnExpression(valueMappingOrFallback);
            var ifEntryExistsShortCircuit = Expression.IfThen(entryExistsTest, returnMapValueResult);

            return Expression.Block(
                dictionaryVariables.Variables,
                ifNoMatchingKeysShortCircuit,
                ifEntryExistsShortCircuit);
        }

        private static bool IsFor(IMemberMapperData mapperData)
        {
            if (mapperData.Context.IsStandalone)
            {
                // A standalone context is either the root, in which case we don't 
                // check the dictionary for a matching object entry, or in a mapper 
                // completing a .Map() call, in which case we already have:
                return false;
            }

            if (!mapperData.SourceMemberIsStringKeyedDictionary(out var dictionarySourceMember))
            {
                return false;
            }

            if (dictionarySourceMember.IsEntireDictionaryMatch)
            {
                // The dictionary has been matched to a target complex type
                // member, so should only be used to get that member's member
                // values, not a value for the member itself
                return false;
            }

            return dictionarySourceMember.HasObjectEntries ||
                   !dictionarySourceMember.ValueType.IsSimple();
        }

        private static Expression GetEntryExistsTest(DictionaryEntryVariablePair dictionaryVariables)
        {
            var returnLabel = Expression.Label(typeof(bool), "Return");
            var returnFalse = Expression.Return(returnLabel, false.ToConstantExpression());

            var ifKeyNotFoundReturnFalse = dictionaryVariables.GetKeyNotFoundShortCircuit(returnFalse);
            var valueAssignment = dictionaryVariables.GetEntryValueAssignment();
            var returnTrue = Expression.Label(returnLabel, true.ToConstantExpression());

            if (dictionaryVariables.HasConstantTargetMemberKey)
            {
                return Expression.Block(ifKeyNotFoundReturnFalse, valueAssignment, returnTrue);
            }

            var keyAssignment = dictionaryVariables.GetNonConstantKeyAssignment();

            return Expression.Block(
                keyAssignment,
                ifKeyNotFoundReturnFalse,
                valueAssignment,
                returnTrue);
        }

        private static Expression GetMapValueCall(Expression sourceValue, IMemberMapperData mapperData)
            => mapperData.Parent.GetRuntimeTypedMapping(sourceValue, mapperData.TargetMember, dataSourceIndex: 0);
    }
}