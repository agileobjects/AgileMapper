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
            .GetMethods(Constants.PublicStatic)
            .First(m => m.Name == "Intersect" && m.GetParameters().Length == 3)
            .MakeGenericMethod(typeof(string));

        private static readonly MethodInfo _linqFirstOrDefaultMethod = typeof(Enumerable)
            .GetMethods(Constants.PublicStatic)
            .First(m => m.Name == "FirstOrDefault" && m.GetParameters().Length == 1)
            .MakeGenericMethod(typeof(string));

        #endregion

        public DictionaryDataSource(MemberMapperData data)
            : this(
                data,
                Expression.Variable(
                    data.SourceType.GetGenericArguments().Last(),
                    data.TargetMember.Name.ToCamelCase()))
        {
        }

        private DictionaryDataSource(MemberMapperData data, ParameterExpression variable)
            : base(
                  new DictionarySourceMember(data),
                  new[] { variable },
                  GetValueParsing(variable, data))
        {
        }

        private static Expression GetValueParsing(Expression variable, MemberMapperData data)
        {
            var potentialNames = GetPotentialNames(data);

            var tryGetValueCall = GetTryGetValueCall(
                variable,
                potentialNames.Select(Expression.Constant),
                data);

            var dictionaryValueOrFallback = Expression.Condition(
                tryGetValueCall,
                GetValue(variable, data),
                GetFallbackValue(variable, potentialNames, data));

            return dictionaryValueOrFallback;
        }

        private static string[] GetPotentialNames(MemberMapperData data)
        {
            var alternateNames = data
                .TargetMember
                .MemberChain
                .Skip(1)
                .Select(data.MapperContext.NamingSettings.GetAlternateNamesFor)
                .CartesianProduct();

            var flattenedNameSet = (data.TargetMember.MemberChain.Count() == 2)
                ? alternateNames.SelectMany(names => names)
                : alternateNames.ToArray().SelectMany(data.MapperContext.NamingSettings.GetJoinedNamesFor);

            return flattenedNameSet.ToArray();
        }

        private static Expression GetTryGetValueCall(
            Expression variable,
            IEnumerable<Expression> potentialNames,
            MemberMapperData data)
        {
            var linqIntersect = Expression.Call(
                _linqIntersectMethod,
                Expression.Property(data.SourceObject, "Keys"),
                Expression.NewArrayInit(typeof(string), potentialNames),
                CaseInsensitiveStringComparer.InstanceMember);

            var intersectionFirstOrDefault = Expression.Call(_linqFirstOrDefaultMethod, linqIntersect);
            var emptyString = Expression.Field(null, typeof(string), "Empty");
            var matchingNameOrEmptyString = Expression.Coalesce(intersectionFirstOrDefault, emptyString);

            var tryGetValueCall = Expression.Call(
                data.SourceObject,
                data.SourceObject.Type.GetMethod("TryGetValue", Constants.PublicInstance),
                matchingNameOrEmptyString,
                variable);

            return tryGetValueCall;
        }

        private static Expression GetValue(Expression variable, MemberMapperData data)
        {
            if (data.TargetMember.IsSimple)
            {
                return data
                    .MapperContext
                    .ValueConverters
                    .GetConversion(variable, data.TargetMember.Type);
            }

            return data.GetMapCall(variable);
        }

        private static Expression GetFallbackValue(
            Expression variable,
            IEnumerable<string> potentialNames,
            MemberMapperData data)
        {
            if (data.TargetMember.IsSimple)
            {
                return data
                    .RuleSet
                    .FallbackDataSourceFactory
                    .Create(data)
                    .Value;
            }

            return GetEnumerablePopulation(variable, potentialNames, data);
        }

        private static Expression GetEnumerablePopulation(
            Expression variable,
            IEnumerable<string> potentialNames,
            MemberMapperData data)
        {
            var sourceElementType = data.SourceType.GetGenericArguments()[1];
            var sourceList = Expression.Variable(typeof(List<>).MakeGenericType(sourceElementType), "sourceList");
            var counter = Expression.Variable(typeof(int), "i");

            var potentialNameConstants = GetPotentialItemNames(potentialNames, counter, data);

            var tryGetValueCall = GetTryGetValueCall(variable, potentialNameConstants, data);
            var loopBreak = Expression.Break(Expression.Label());
            var ifNotTryGetValueBreak = Expression.IfThen(Expression.Not(tryGetValueCall), loopBreak);

            var sourceListAddCall = Expression.Call(sourceList, "Add", Constants.NoTypeArguments, variable);
            var incrementCounter = Expression.PreIncrementAssign(counter);

            var loopBody = Expression.Block(
                ifNotTryGetValueBreak,
                sourceListAddCall,
                incrementCounter);

            var populationLoop = Expression.Loop(loopBody, loopBreak.Target);

            var mapCall = data.GetMapCall(sourceList);

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
            MemberMapperData data)
        {
            return potentialNames
                .Select(name =>
                {
                    var nameAndOpenBrace = Expression.Constant(name + "[");
                    var counterString = data.MapperContext.ValueConverters.GetConversion(counter, typeof(string));
                    var closeBrace = Expression.Constant("]");

                    var stringConcatMethod = typeof(string)
                        .GetMethods(Constants.PublicStatic)
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