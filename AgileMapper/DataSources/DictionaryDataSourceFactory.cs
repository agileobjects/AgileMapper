namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;
    using Members;
    using ReadableExpressions.Extensions;


    internal class DictionaryDataSourceFactory : IMaptimeDataSourceFactory
    {
        public bool IsFor(IMemberMapperData mapperData) => mapperData.HasUseableSourceDictionary();

        public IDataSource Create(IChildMemberMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var sourceMember = new DictionarySourceMember(mapperData);

            if (mapperData.TargetMember.IsSimple)
            {
                return new DictionaryEntryDataSource(sourceMember, mapperData);
            }

            return new DictionaryDataSource(sourceMember, mapperData);
        }

        private class DictionaryEntryDataSource : DataSourceBase
        {
            private readonly DictionaryEntryVariablePair _dictionaryVariables;

            public DictionaryEntryDataSource(DictionarySourceMember sourceMember, IMemberMapperData childMapperData)
                : this(
                      sourceMember.EntryMember,
                      new DictionaryEntryVariablePair(sourceMember, childMapperData),
                      childMapperData)
            {
            }

            private DictionaryEntryDataSource(
                IQualifiedMember sourceMember,
                DictionaryEntryVariablePair dictionaryVariables,
                IMemberMapperData childMapperData)
                : base(
                    sourceMember,
                    new[] { dictionaryVariables.Key },
                    GetDictionaryEntryValue(dictionaryVariables, childMapperData),
                    GetMatchingKeyExistsTest(dictionaryVariables, childMapperData))
            {
                _dictionaryVariables = dictionaryVariables;
            }

            private static Expression GetDictionaryEntryValue(
                DictionaryEntryVariablePair dictionaryVariables,
                IMemberMapperData childMapperData)
            {
                var dictionaryEntryAccess = dictionaryVariables.GetEntryValueAccess(childMapperData);
                var targetType = childMapperData.TargetMember.Type;

                if (targetType.IsAssignableFrom(dictionaryEntryAccess.Type))
                {
                    return dictionaryEntryAccess;
                }

                var valueVariableAssignment = Expression.Assign(dictionaryVariables.Value, dictionaryEntryAccess);

                var valueConversion = childMapperData
                    .MapperContext
                    .ValueConverters
                    .GetConversion(dictionaryVariables.Value, targetType);

                if (dictionaryVariables.SourceMember.EntryType.CanBeNull())
                {
                    valueConversion = Expression.Condition(
                        dictionaryVariables.Value.GetIsNotDefaultComparison(),
                        valueConversion,
                        Expression.Default(targetType));
                }

                return Expression.Block(new[] { dictionaryVariables.Value }, valueVariableAssignment, valueConversion);
            }

            private static Expression GetMatchingKeyExistsTest(
                DictionaryEntryVariablePair dictionaryVariables,
                IMemberMapperData mapperData)
            {
                var keyVariableAssignment = dictionaryVariables.GetMatchingKeyAssignment(mapperData);

                return keyVariableAssignment.GetIsNotDefaultComparison();
            }

            public override Expression AddCondition(Expression value)
            {
                if (_dictionaryVariables.HasConstantTargetMemberKey)
                {
                    return base.AddCondition(value);
                }

                return Expression.Block(
                    _dictionaryVariables.GetKeyAssignment(_dictionaryVariables.TargetMemberKey),
                    base.AddCondition(value));
            }
        }

        private class DictionaryDataSource : DataSourceBase
        {
            public DictionaryDataSource(IQualifiedMember sourceMember, IMemberMapperData mapperData)
                : base(
                      sourceMember,
                      sourceMember.GetQualifiedAccess(mapperData.SourceObject))
            {
            }
        }
    }
}