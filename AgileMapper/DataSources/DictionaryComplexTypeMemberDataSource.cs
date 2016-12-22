namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class DictionaryComplexTypeMemberDataSource : DataSourceBase, IMaptimeDataSource
    {
        public DictionaryComplexTypeMemberDataSource(
            DictionarySourceMember sourceMember,
            IChildMemberMappingData childMappingData)
            : this(
                new DictionaryEntryVariablePair(sourceMember, childMappingData.MapperData),
                childMappingData)
        {
        }

        private DictionaryComplexTypeMemberDataSource(
            DictionaryEntryVariablePair dictionaryVariables,
            IChildMemberMappingData childMappingData)
            : base(
                dictionaryVariables.SourceMember,
                dictionaryVariables.Variables,
                GetDictionaryEntryValue(dictionaryVariables, childMappingData),
                GetEntryExistsTest(dictionaryVariables))
        {
        }

        private static Expression GetDictionaryEntryValue(
            DictionaryEntryVariablePair dictionaryVariables,
            IChildMemberMappingData childMappingData)
        {
            var mapperData = dictionaryVariables.MapperData;
            var foundValueNonNull = dictionaryVariables.Value.GetIsNotDefaultComparison();

            var valueMapCall = mapperData.Parent.GetMapCall(
                dictionaryVariables.Value,
                mapperData.TargetMember,
                dataSourceIndex: 0);

            var fallbackValue = childMappingData
                .RuleSet
                .FallbackDataSourceFactory
                .Create(mapperData).Value;

            var valueMappingOrFallback = Expression.Condition(foundValueNonNull, valueMapCall, fallbackValue);

            return valueMappingOrFallback;
        }

        private static Expression GetEntryExistsTest(DictionaryEntryVariablePair dictionaryVariables)
        {
            var returnLabel = Expression.Label(typeof(bool), "Return");
            var returnFalse = Expression.Return(returnLabel, Expression.Constant(false, typeof(bool)));

            var ifKeyNotFoundReturnFalse = dictionaryVariables.GetKeyNotFoundShortCircuit(returnFalse);
            var valueAssignment = dictionaryVariables.GetEntryValueAssignment();
            var returnTrue = Expression.Label(returnLabel, Expression.Constant(true, typeof(bool)));

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

        public bool WrapInFinalDataSource => false;
    }
}