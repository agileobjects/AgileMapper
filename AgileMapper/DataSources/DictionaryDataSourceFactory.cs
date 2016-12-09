namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;
#if NET_STANDARD
    using System.Reflection;
#endif

    internal class DictionaryDataSourceFactory : IMaptimeDataSourceFactory
    {
        public bool IsFor(IMemberMapperData mapperData) => mapperData.HasSourceDictionary();

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

                return Expression.Block(new[] { dictionaryVariables.Value }, valueVariableAssignment, valueConversion);
            }

            private static Expression GetMatchingKeyExistsTest(
                DictionaryEntryVariablePair dictionaryVariables,
                IMemberMapperData mapperData)
            {
                var keyVariableAssignment = dictionaryVariables.GetMatchingKeyAssignment(mapperData);

                return keyVariableAssignment.GetIsNotDefaultComparison();
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