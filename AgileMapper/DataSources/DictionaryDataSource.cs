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

        public DictionaryDataSource(IMemberMapperData mapperData)
            : this(
                mapperData,
                Expression.Variable(
                    mapperData.SourceType.GetGenericArguments().Last(),
                    mapperData.TargetMember.Name.ToCamelCase()))
        {
        }

        private DictionaryDataSource(IMemberMapperData mapperData, ParameterExpression variable)
            : base(
                  new DictionarySourceMember(mapperData),
                  new[] { variable },
                  GetValueParsing(variable, mapperData))
        {
        }

        private static Expression GetValueParsing(Expression variable, IMemberMapperData mapperData)
        {
            var potentialNames = GetPotentialNames(mapperData);

            var tryGetValueCall = GetTryGetValueCall(
                variable,
                potentialNames.Select(Expression.Constant),
                mapperData);

            var dictionaryValueOrFallback = Expression.Condition(
                tryGetValueCall,
                GetValue(variable, mapperData),
                GetFallbackValue(variable, potentialNames, mapperData));

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

        private static Expression GetValue(Expression variable, IMemberMapperData mapperData)
        {
            if (mapperData.TargetMember.IsSimple)
            {
                return mapperData
                    .MapperContext
                    .ValueConverters
                    .GetConversion(variable, mapperData.TargetMember.Type);
            }

            return mapperData.GetMapCall(variable);
        }

        private static Expression GetFallbackValue(
            Expression variable,
            IEnumerable<string> potentialNames,
            IMemberMapperData mapperData)
        {
            if (mapperData.TargetMember.IsSimple)
            {
                return mapperData
                    .RuleSet
                    .FallbackDataSourceFactory
                    .Create(mapperData)
                    .Value;
            }

            return GetEnumerablePopulation(variable, potentialNames, mapperData);
        }

        private static Expression GetEnumerablePopulation(
            Expression variable,
            IEnumerable<string> potentialNames,
            IMemberMapperData mapperData)
        {
            var sourceElementType = mapperData.SourceType.GetGenericArguments()[1];
            var sourceList = Expression.Variable(typeof(List<>).MakeGenericType(sourceElementType), "sourceList");
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

            var mapCall = mapperData.GetMapCall(sourceList);

            var enumerablePopulation = Expression.Block(
                new[] { sourceList, counter },
                Expression.Assign(sourceList, sourceList.Type.GetEmptyInstanceCreation()),
                Expression.Assign(counter, Expression.Constant(0)),
                populationLoop,
                mapCall);

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