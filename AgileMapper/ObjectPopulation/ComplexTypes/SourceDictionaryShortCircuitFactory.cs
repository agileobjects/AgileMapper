namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using DataSources;
    using Extensions.Internal;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class SourceDictionaryShortCircuitFactory : ISourceShortCircuitFactory
    {
        public static readonly ISourceShortCircuitFactory Instance = new SourceDictionaryShortCircuitFactory();

        public bool IsFor(ObjectMapperData mapperData)
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

        public Expression GetShortCircuit(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            var dictionaryVariables = new DictionaryEntryVariablePair(mapperData);
            var foundValueNonNull = dictionaryVariables.Value.GetIsNotDefaultComparison();

            var entryExistsTest = GetEntryExistsTest(dictionaryVariables);

            var mapValueCall = GetMapValueCall(dictionaryVariables.Value, mapperData);
            var fallbackValue = GetFallbackValue(mappingData);

            var valueMappingOrFallback = Expression.Condition(foundValueNonNull, mapValueCall, fallbackValue);
            var returnMapValueResult = Expression.Return(mapperData.ReturnLabelTarget, valueMappingOrFallback);
            var ifEntryExistsShortCircuit = Expression.IfThen(entryExistsTest, returnMapValueResult);

            return Expression.Block(dictionaryVariables.Variables, ifEntryExistsShortCircuit);
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

        private static MethodCallExpression GetMapValueCall(Expression sourceValue, IMemberMapperData mapperData)
            => mapperData.Parent.GetMapCall(sourceValue, mapperData.TargetMember, dataSourceIndex: 0);

        private static Expression GetFallbackValue(IObjectMappingData mappingData)
        {
            return mappingData.MappingContext
                .RuleSet
                .FallbackDataSourceFactory
                .Create(mappingData.MapperData)
                .Value;
        }
    }
}