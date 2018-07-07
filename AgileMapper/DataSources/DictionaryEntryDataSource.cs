namespace AgileObjects.AgileMapper.DataSources
{
    using Extensions.Internal;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class DictionaryEntryDataSource : DataSourceBase
    {
        private readonly DictionaryEntryVariablePair _dictionaryVariables;
        private Expression _preCondition;

        public DictionaryEntryDataSource(DictionaryEntryVariablePair dictionaryVariables)
            : base(
                dictionaryVariables.SourceMember.EntryMember,
                dictionaryVariables.Variables,
                GetDictionaryEntryValue(dictionaryVariables),
                GetValidEntryExistsTest(dictionaryVariables))
        {
            _dictionaryVariables = dictionaryVariables;
        }

        private static Expression GetDictionaryEntryValue(DictionaryEntryVariablePair dictionaryVariables)
        {
            if (dictionaryVariables.UseDirectValueAccess)
            {
                return dictionaryVariables.GetEntryValueAccess();
            }

            var valueConversion = dictionaryVariables.MapperData
                .MapperContext
                .ValueConverters
                .GetConversion(dictionaryVariables.Value, dictionaryVariables.MapperData.TargetMember.Type);

            return valueConversion;
        }

        private static Expression GetValidEntryExistsTest(DictionaryEntryVariablePair dictionaryVariables)
        {
            if (dictionaryVariables.UseDirectValueAccess)
            {
                return null;
            }

            var valueVariableAssignment = dictionaryVariables.GetEntryValueAssignment();
            var valueNonNull = valueVariableAssignment.GetIsNotDefaultComparison();

            return valueNonNull;
        }

        public override Expression PreCondition => _preCondition ?? (_preCondition = CreatePreCondition());

        private Expression CreatePreCondition()
        {
            var matchingKeyExists = GetMatchingKeyExistsTest();

            if (_dictionaryVariables.HasConstantTargetMemberKey)
            {
                return matchingKeyExists;
            }

            var keyAssignment = GetNonConstantKeyAssignment();

            return Expression.Block(keyAssignment, matchingKeyExists);
        }

        public override Expression AddPreCondition(Expression population)
        {
            var matchingKeyExists = GetMatchingKeyExistsTest();
            var ifKeyExistsPopulate = Expression.IfThen(matchingKeyExists, population);

            if (_dictionaryVariables.HasConstantTargetMemberKey)
            {
                return ifKeyExistsPopulate;
            }

            var keyAssignment = GetNonConstantKeyAssignment();

            return Expression.Block(keyAssignment, ifKeyExistsPopulate);
        }

        private Expression GetMatchingKeyExistsTest()
        {
            var keyVariableAssignment = _dictionaryVariables.GetMatchingKeyAssignment();
            var matchingKeyExists = keyVariableAssignment.GetIsNotDefaultComparison();

            return matchingKeyExists;
        }

        private Expression GetNonConstantKeyAssignment()
            => _dictionaryVariables.GetNonConstantKeyAssignment();
    }
}