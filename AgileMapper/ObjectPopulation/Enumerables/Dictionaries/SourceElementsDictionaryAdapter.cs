namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.Dictionaries
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
    using Extensions.Internal;
    using Members.Dictionaries;
    using NetStandardPolyfills;

    internal class SourceElementsDictionaryAdapter : SourceEnumerableAdapterBase, ISourceEnumerableAdapter
    {
        private readonly DictionaryEntryVariablePair _dictionaryVariables;

        public SourceElementsDictionaryAdapter(
            DictionarySourceMember sourceMember,
            EnumerablePopulationBuilder builder)
            : base(builder)
        {
            _dictionaryVariables = new DictionaryEntryVariablePair(sourceMember, builder.MapperData);
        }

        private DictionarySourceMember SourceMember => _dictionaryVariables.SourceMember;

        public override Expression GetSourceValues()
        {
            var dictionaryAccess = base.GetSourceValues();

            if (!Builder.ElementTypesAreSimple)
            {
                return Expression.Property(dictionaryAccess, "Values");
            }

            var kvpType = typeof(KeyValuePair<,>)
                .MakeGenericType(SourceMember.KeyType, SourceMember.ValueType);

            var kvpParameter = Parameters.Create(kvpType, "kvp");

            var targetEnumerableKey = Builder.MapperData.GetTargetMemberDictionaryKey();

            var elementKeyPrefix = Builder.MapperData.MapperContext
                .UserConfigurations
                .Dictionaries
                .GetElementKeyPrefixOrNull(Builder.MapperData);

            var targetEnumerableElementKey = (elementKeyPrefix != null)
                ? new List<Expression> { targetEnumerableKey, elementKeyPrefix }.GetStringConcatCall()
                : targetEnumerableKey;

            var keyMatchesQuery = _dictionaryVariables.GetKeyStartsWithIgnoreCaseCall(
                Expression.Property(kvpParameter, "Key"),
                targetEnumerableElementKey);

            var keyMatchesLambda = Expression.Lambda(
                Expression.GetFuncType(kvpType, typeof(bool)),
                keyMatchesQuery,
                kvpParameter);

            var linqWhereMethod = typeof(Enumerable)
                .GetPublicStaticMethods("Where")
                .First(m => m.GetParameters()[1].ParameterType.GetGenericTypeArguments().Length == 2)
                .MakeGenericMethod(kvpType);

            var filteredEntries = Expression.Call(linqWhereMethod, dictionaryAccess, keyMatchesLambda);

            var linqSelectMethod = typeof(Enumerable)
                .GetPublicStaticMethods("Select")
                .First(m => m.GetParameters()[1].ParameterType.GetGenericTypeArguments().Length == 2)
                .MakeGenericMethod(kvpType, SourceMember.ValueType);

            var kvpValueLambda = Expression.Lambda(
                Expression.GetFuncType(kvpType, SourceMember.ValueType),
                Expression.Property(kvpParameter, "Value"),
                kvpParameter);

            var filteredValues = Expression.Call(linqSelectMethod, filteredEntries, kvpValueLambda);

            return filteredValues;
        }

        public Expression GetSourceCountAccess()
            => Expression.Property(SourceValue, "Count");

        public override bool UseReadOnlyTargetWrapper
            => base.UseReadOnlyTargetWrapper && Builder.Context.ElementTypesAreSimple;

        public Expression GetMappingShortCircuitOrNull() => null;

        public IPopulationLoopData GetPopulationLoopData()
        {
            if (Builder.ElementTypesAreSimple)
            {
                return new EnumerableSourcePopulationLoopData(Builder);
            }

            return new SourceElementsDictionaryPopulationLoopData(_dictionaryVariables, Builder);
        }
    }
}