namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;

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

        public DictionaryDataSource(IChildMemberMappingData childMappingData)
            : this(
                new DictionarySourceMember(childMappingData.MapperData),
                Expression.Variable(
                    childMappingData.MapperData.SourceType.GetGenericArguments().Last(),
                    childMappingData.MapperData.TargetMember.Name.ToCamelCase()),
                childMappingData)
        {
        }

        private DictionaryDataSource(
            DictionarySourceMember sourceMember,
            ParameterExpression variable,
            IChildMemberMappingData childMappingData)
            : base(
                  sourceMember,
                  new[] { variable },
                  GetValueParsing(sourceMember, variable, childMappingData))
        {
        }

        private static Expression GetValueParsing(
            DictionarySourceMember sourceMember,
            Expression variable,
            IChildMemberMappingData childMappingData)
        {
            var childMapperData = childMappingData.MapperData;
            var potentialNames = GetPotentialNames(childMapperData);
            var fallbackValue = GetFallbackValue(sourceMember, variable, potentialNames, childMappingData);

            if ((childMapperData.TargetMember.IsEnumerable != variable.Type.IsEnumerable()) &&
                (variable.Type != typeof(object)))
            {
                return fallbackValue;
            }

            var tryGetValueCall = GetTryGetValueCall(
                variable,
                potentialNames.Select(Expression.Constant),
                childMapperData);

            var dictionaryValueOrFallback = Expression.Condition(
                tryGetValueCall,
                GetValue(sourceMember, variable, childMappingData),
                fallbackValue);

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
            IMemberMapperData childMapperData)
        {
            var linqIntersect = Expression.Call(
                _linqIntersectMethod,
                Expression.Property(childMapperData.SourceObject, "Keys"),
                Expression.NewArrayInit(typeof(string), potentialNames),
                Expression.Property(null, typeof(StringComparer), "OrdinalIgnoreCase"));

            var intersectionFirstOrDefault = Expression.Call(_linqFirstOrDefaultMethod, linqIntersect);
            var emptyString = Expression.Field(null, typeof(string), "Empty");
            var matchingNameOrEmptyString = Expression.Coalesce(intersectionFirstOrDefault, emptyString);

            var tryGetValueCall = Expression.Call(
                childMapperData.SourceObject,
                childMapperData.SourceObject.Type.GetMethod("TryGetValue"),
                matchingNameOrEmptyString,
                variable);

            return tryGetValueCall;
        }

        private static Expression GetValue(
            IQualifiedMember sourceMember,
            Expression variable,
            IChildMemberMappingData childMappingData)
        {
            var childMapperData = childMappingData.MapperData;

            if (childMapperData.TargetMember.IsSimple)
            {
                return childMapperData
                    .MapperContext
                    .ValueConverters
                    .GetConversion(variable, childMapperData.TargetMember.Type);
            }

            var entrySourceMember = sourceMember.WithType(variable.Type);

            var mapping = MappingFactory.GetChildMapping(
                entrySourceMember,
                variable,
                0,
                childMappingData);

            return mapping;
        }

        private static Expression GetFallbackValue(
            DictionarySourceMember sourceMember,
            Expression variable,
            IEnumerable<string> potentialNames,
            IChildMemberMappingData childMappingData)
        {
            if (DictionaryEntriesCouldBeEnumerableElements(sourceMember, childMappingData))
            {
                return GetEnumerablePopulation(
                    sourceMember,
                    variable,
                    potentialNames,
                    childMappingData);
            }

            return childMappingData
                .RuleSet
                .FallbackDataSourceFactory
                .Create(childMappingData)
                .Value;
        }

        private static bool DictionaryEntriesCouldBeEnumerableElements(
            DictionarySourceMember sourceMember,
            IChildMemberMappingData childMappingData)
        {
            if (!childMappingData.MapperData.TargetMember.IsEnumerable)
            {
                return false;
            }

            if (sourceMember.EntryType == typeof(object))
            {
                return true;
            }

            var targetElementsAreCompatibleWithEntries = childMappingData.MapperData
                .TargetMember.ElementType
                .IsAssignableFrom(sourceMember.EntryType);

            return targetElementsAreCompatibleWithEntries;
        }

        private static Expression GetEnumerablePopulation(
            DictionarySourceMember sourceMember,
            Expression variable,
            IEnumerable<string> potentialNames,
            IChildMemberMappingData childMappingData)
        {
            var sourceList = Expression.Variable(typeof(List<>).MakeGenericType(sourceMember.EntryType), "sourceList");
            var counter = Expression.Variable(typeof(int), "i");

            var potentialNameConstants = GetPotentialItemNames(potentialNames, counter, childMappingData.MapperData);

            var tryGetValueCall = GetTryGetValueCall(variable, potentialNameConstants, childMappingData.MapperData);
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

            var mapping = MappingFactory.GetChildMapping(
                entrySourceMember,
                sourceList,
                0,
                childMappingData);

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
            IMemberMapperData childMapperData)
        {
            return potentialNames
                .Select(name =>
                {
                    var nameAndOpenBrace = Expression.Constant(name + "[");
                    var counterString = childMapperData.MapperContext.ValueConverters.GetConversion(counter, typeof(string));
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