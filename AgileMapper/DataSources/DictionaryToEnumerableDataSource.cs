namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;

    internal class DictionaryToEnumerableDataSource : DataSourceBase
    {
        private static readonly MethodInfo _stringConcatMethod = typeof(string)
            .GetPublicStaticMethods()
            .First(m => (m.Name == "Concat") &&
                        (m.GetParameters().Length == 3) &&
                        (m.GetParameters().First().ParameterType == typeof(string)));

        public DictionaryToEnumerableDataSource(
            DictionarySourceMember sourceMember,
            IChildMemberMappingData childMappingData)
            : base(sourceMember, GetEnumerablePopulation(sourceMember, childMappingData))
        {
        }

        private static Expression GetEnumerablePopulation(
            DictionarySourceMember sourceMember,
            IChildMemberMappingData childMappingData)
        {
            var childMapperData = childMappingData.MapperData;
            var context = new EnumerablePopulationContext(childMapperData);

            var sourceList = context.GetSourceParameterFor(typeof(List<>).MakeGenericType(sourceMember.EntryType));
            var counter = Expression.Variable(typeof(int), "i");
            var targetMemberKey = GetTargetMemberKey(counter, childMapperData);

            var variablePair = new DictionaryEntryVariablePair(sourceMember, childMapperData);

            var matchingKeyAssignment = DictionaryEntryDataSource.GetMatchingKeyAssignment(
                variablePair.Key,
                targetMemberKey,
                childMappingData);

            // TODO: Look into duplication between this and EnumerablePopulationBuilder
            var breakLoop = Expression.Break(Expression.Label(typeof(void), "Break"));
            var noMatchingKey = matchingKeyAssignment.GetIsDefaultComparison();
            var ifNotTryGetValueBreak = Expression.IfThen(noMatchingKey, breakLoop);

            var dictionaryEntry = childMapperData.SourceObject.GetIndexAccess(variablePair.Key);
            var sourceListAddCall = Expression.Call(sourceList, "Add", Constants.NoTypeArguments, dictionaryEntry);
            var incrementCounter = Expression.PreIncrementAssign(counter);

            var loopBody = Expression.Block(
                ifNotTryGetValueBreak,
                sourceListAddCall,
                incrementCounter);

            var populationLoop = Expression.Loop(loopBody, breakLoop.Target);

            var entrySourceMember = sourceMember.WithType(sourceList.Type);

            var mapping = MappingFactory.GetChildMapping(
                entrySourceMember,
                sourceList,
                0, // <- dataSourceIndex
                childMappingData);

            var enumerablePopulation = Expression.Block(
                new[] { sourceList, variablePair.Key, counter },
                Expression.Assign(sourceList, sourceList.Type.GetEmptyInstanceCreation()),
                Expression.Assign(counter, Expression.Constant(0)),
                populationLoop,
                mapping);

            return enumerablePopulation;
        }

        private static Expression GetTargetMemberKey(Expression counter, IMemberMapperData childMapperData)
        {
            var nameAndOpenBrace = Expression.Constant(childMapperData.TargetMember.Name + "[");
            var counterString = childMapperData.MapperContext.ValueConverters.GetConversion(counter, typeof(string));
            var closeBrace = Expression.Constant("]");

            var nameConstant = Expression.Call(
                null,
                _stringConcatMethod,
                nameAndOpenBrace,
                counterString,
                closeBrace);

            return nameConstant;
        }
    }
}