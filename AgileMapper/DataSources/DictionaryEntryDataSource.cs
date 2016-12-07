namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;

    internal class DictionaryEntryDataSource : DataSourceBase
    {
        private static readonly MethodInfo _linqFirstOrDefaultMethod = typeof(Enumerable)
            .GetPublicStaticMethods()
            .First(m => (m.Name == "FirstOrDefault") && (m.GetParameters().Length == 2))
            .MakeGenericMethod(typeof(string));

        private static readonly MethodInfo _stringEqualsMethod = typeof(string)
            .GetPublicInstanceMethods()
            .First(m => (m.Name == "Equals") && (m.GetParameters().Length == 2));

        public DictionaryEntryDataSource(DictionarySourceMember sourceMember, IChildMemberMappingData childMappingData)
            : this(sourceMember, GetTargetMemberKeyFor(childMappingData.MapperData), childMappingData)
        {
        }

        private static Expression GetTargetMemberKeyFor(IBasicMapperData childMapperData)
        {
            var joinedName = string.Join(
                string.Empty,
                childMapperData.TargetMember.MemberChain.Skip(1).Select(m => m.JoiningName));

            if (joinedName.StartsWith('.'))
            {
                joinedName = joinedName.Substring(1);
            }

            return Expression.Constant(joinedName, typeof(string));
        }

        public DictionaryEntryDataSource(
            DictionarySourceMember sourceMember,
            Expression targetMemberKey,
            IChildMemberMappingData childMappingData)
            : this(
                sourceMember,
                new DictionaryEntryVariablePair(sourceMember, childMappingData.MapperData),
                targetMemberKey,
                childMappingData)
        {
        }

        private DictionaryEntryDataSource(
            IQualifiedMember sourceMember,
            DictionaryEntryVariablePair variablePair,
            Expression targetMemberKey,
            IChildMemberMappingData childMappingData)
            : base(
                sourceMember,
                new[] { variablePair.Key },
                GetDictionaryEntryValue(sourceMember, variablePair, childMappingData),
                GetMatchingKeyExistsTest(variablePair.Key, targetMemberKey, childMappingData))
        {
        }

        private static Expression GetDictionaryEntryValue(
            IQualifiedMember sourceMember,
            DictionaryEntryVariablePair variablePair,
            IChildMemberMappingData childMappingData)
        {
            var childMapperData = childMappingData.MapperData;
            var dictionaryAccess = childMapperData.SourceObject.GetIndexAccess(variablePair.Key);

            var targetType = childMapperData.TargetMember.Type;

            if (targetType.IsAssignableFrom(dictionaryAccess.Type))
            {
                return dictionaryAccess;
            }

            if (childMapperData.TargetMember.IsSimple)
            {
                var valueVariableAssignment = Expression.Assign(variablePair.Value, dictionaryAccess);

                var valueConversion = childMapperData
                    .MapperContext
                    .ValueConverters
                    .GetConversion(variablePair.Value, targetType);

                return Expression.Block(new[] { variablePair.Value }, valueVariableAssignment, valueConversion);
            }

            var entrySourceMember = sourceMember.WithType(dictionaryAccess.Type);

            return MappingFactory.GetChildMapping(
                entrySourceMember,
                dictionaryAccess,
                0, // <- dataSourceIndex
                childMappingData);
        }

        private static Expression GetMatchingKeyExistsTest(
            Expression keyVariable,
            Expression targetMemberKey,
            IChildMemberMappingData childMappingData)
        {
            var keyVariableAssignment = GetMatchingKeyAssignment(keyVariable, targetMemberKey, childMappingData);

            return keyVariableAssignment.GetIsNotDefaultComparison();
        }

        public static Expression GetMatchingKeyAssignment(
            Expression keyVariable,
            Expression targetMemberKey,
            IChildMemberMappingData childMappingData)
        {
            var keyParameter = Expression.Parameter(typeof(string), "key");

            var parameterEqualsTargetKey = Expression.Call(
                keyParameter,
                _stringEqualsMethod,
                targetMemberKey,
                Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison)));

            var keyMatchesLambda = Expression.Lambda<Func<string, bool>>(parameterEqualsTargetKey, keyParameter);

            var dictionaryKeys = Expression.Property(childMappingData.MapperData.SourceObject, "Keys");

            var firstMatchingKeyOrNull = Expression.Call(
                _linqFirstOrDefaultMethod,
                dictionaryKeys,
                keyMatchesLambda);

            var keyVariableAssignment = Expression.Assign(keyVariable, firstMatchingKeyOrNull);

            return keyVariableAssignment;
        }
    }
}