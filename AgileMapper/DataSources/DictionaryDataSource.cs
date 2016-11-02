namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;

    internal class DictionaryDataSource : DataSourceBase
    {
        #region Cached Items

        private static readonly MethodInfo _linqIntersectMethod = typeof(Enumerable)
            .GetPublicStaticMethods()
            .First(m => m.Name == "Intersect" && m.GetParameters().Length == 3)
            .MakeGenericMethod(typeof(string));

        private static readonly MethodInfo _linqFirstOrDefaultMethod = typeof(Enumerable)
            .GetPublicStaticMethods()
            .First(m => m.Name == "FirstOrDefault" && m.GetParameters().Length == 1)
            .MakeGenericMethod(typeof(string));

        #endregion

        public DictionaryDataSource(IMemberMappingData mappingData)
            : this(
                new DictionarySourceMember(mappingData.MapperData),
                Expression.Variable(
                    mappingData.MapperData.SourceType.GetGenericArguments().Last(),
                    mappingData.MapperData.TargetMember.Name.ToCamelCase()),
                mappingData)
        {
        }

        private DictionaryDataSource(
            DictionarySourceMember sourceMember,
            ParameterExpression variable,
            IMemberMappingData mappingData)
            : base(
                  sourceMember,
                  new[] { variable },
                  GetValueParsing(sourceMember, variable, mappingData))
        {
        }

        private static Expression GetValueParsing(
            DictionarySourceMember sourceMember,
            Expression variable,
            IMemberMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var potentialNames = GetPotentialNames(mapperData);

            var tryGetValueCall = GetTryGetValueCall(
                variable,
                potentialNames.Select(Expression.Constant),
                mapperData);

            var dictionaryValueOrFallback = Expression.Condition(
                tryGetValueCall,
                GetValue(sourceMember, variable, mappingData),
                GetFallbackValue(sourceMember, variable, potentialNames, mappingData));

            return dictionaryValueOrFallback;
        }

        private static string[] GetPotentialNames(IMemberMapperData mapperData)
        {
            var alternateNames = mapperData
                .TargetMember
                .MemberChain
                .Skip(1)
                .Select(mapperData.MapperContext.NamingSettings.GetAlternateNamesFor)
                .CartesianProduct();

            var flattenedNameSet = (mapperData.TargetMember.MemberChain.Count() == 2)
                ? alternateNames.SelectMany(names => names)
                : alternateNames.ToArray().SelectMany(mapperData.MapperContext.NamingSettings.GetJoinedNamesFor);

            return flattenedNameSet.ToArray();
        }

        private static Expression GetTryGetValueCall(
            Expression variable,
            IEnumerable<Expression> potentialNames,
            IMemberMapperData mapperData)
        {
            var linqIntersect = Expression.Call(
                _linqIntersectMethod,
                Expression.Property(mapperData.SourceObject, "Keys"),
                Expression.NewArrayInit(typeof(string), potentialNames),
                CaseInsensitiveStringComparer.InstanceMember);

            var intersectionFirstOrDefault = Expression.Call(_linqFirstOrDefaultMethod, linqIntersect);
            var emptyString = Expression.Field(null, typeof(string), "Empty");
            var matchingNameOrEmptyString = Expression.Coalesce(intersectionFirstOrDefault, emptyString);

            var tryGetValueCall = Expression.Call(
                mapperData.SourceObject,
                mapperData.SourceObject.Type.GetMethod("TryGetValue"),
                matchingNameOrEmptyString,
                variable);

            return tryGetValueCall;
        }

        private static Expression GetValue(
            IQualifiedMember sourceMember,
            Expression variable,
            IMemberMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (mapperData.TargetMember.IsSimple)
            {
                return mapperData
                    .MapperContext
                    .ValueConverters
                    .GetConversion(variable, mapperData.TargetMember.Type);
            }

            var entrySourceMember = sourceMember.WithType(variable.Type);

            var mapping = InlineMappingFactory.GetChildMapping(
                entrySourceMember,
                variable,
                0,
                mappingData);

            return mapping;
        }

        private static Expression GetFallbackValue(
            DictionarySourceMember sourceMember,
            Expression variable,
            IEnumerable<string> potentialNames,
            IMemberMappingData mappingData)
        {
            if (DictionaryEntriesCouldBeEnumerableElements(sourceMember, mappingData))
            {
                return GetEnumerablePopulation(
                    sourceMember,
                    variable,
                    potentialNames,
                    mappingData);
            }

            return mappingData
                .RuleSet
                .FallbackDataSourceFactory
                .Create(mappingData)
                .Value;
        }

        private static bool DictionaryEntriesCouldBeEnumerableElements(
            DictionarySourceMember sourceMember,
            IMemberMappingData mappingData)
        {
            if (!mappingData.MapperData.TargetMember.IsEnumerable)
            {
                return false;
            }

            if (sourceMember.EntryType == typeof(object))
            {
                return true;
            }

            var targetElementsAreCompatibleWithEntries = mappingData.MapperData
                .TargetMember.ElementType
                .IsAssignableFrom(sourceMember.EntryType);

            return targetElementsAreCompatibleWithEntries;
        }

        private static Expression GetEnumerablePopulation(
            DictionarySourceMember sourceMember,
            Expression variable,
            IEnumerable<string> potentialNames,
            IMemberMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var sourceList = Expression.Variable(typeof(List<>).MakeGenericType(sourceMember.EntryType), "sourceList");
            var counter = Expression.Variable(typeof(int), "i");

            var potentialNameConstants = GetPotentialItemNames(potentialNames, counter, mapperData);

            var tryGetValueCall = GetTryGetValueCall(variable, potentialNameConstants, mapperData);
            var loopBreak = Expression.Break(Expression.Label());
            var ifNotTryGetValueBreak = Expression.IfThen(Expression.Not(tryGetValueCall), loopBreak);

            var sourceListAddCall = Expression.Call(sourceList, "Add", Constants.NoTypeArguments, variable);
            var incrementCounter = Expression.PreIncrementAssign(counter);

            var loopBody = Expression.Block(
                ifNotTryGetValueBreak,
                sourceListAddCall,
                incrementCounter);

            var populationLoop = Expression.Loop(loopBody, loopBreak.Target);

            var entrySourceMember = sourceMember.WithType(sourceList.Type);

            var mapping = InlineMappingFactory.GetChildMapping(
                entrySourceMember,
                sourceList,
                0,
                mappingData);

            var enumerablePopulation = Expression.Block(
                new[] { sourceList, counter },
                Expression.Assign(sourceList, sourceList.Type.GetEmptyInstanceCreation()),
                Expression.Assign(counter, Expression.Constant(0)),
                populationLoop,
                mapping);

            return enumerablePopulation;
        }

        private static IEnumerable<MethodCallExpression> GetPotentialItemNames(
            IEnumerable<string> potentialNames,
            Expression counter,
            IMemberMapperData mapperData)
        {
            return potentialNames
                .Select(name =>
                {
                    var nameAndOpenBrace = Expression.Constant(name + "[");
                    var counterString = mapperData.MapperContext.ValueConverters.GetConversion(counter, typeof(string));
                    var closeBrace = Expression.Constant("]");

                    var stringConcatMethod = typeof(string)
                        .GetPublicStaticMethods()
                        .First(m => (m.Name == "Concat") &&
                                    (m.GetParameters().Length == 3) &&
                                    (m.GetParameters().First().ParameterType == typeof(string)));

                    var nameConstant = Expression.Call(
                        null,
                        stringConcatMethod,
                        nameAndOpenBrace,
                        counterString,
                        closeBrace);

                    return nameConstant;
                })
                .ToArray();
        }
    }
}