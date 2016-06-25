namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;

    internal class DictionaryDataSourceFactory : IConditionalDataSourceFactory
    {
        public bool IsFor(IMemberMappingContext context)
        {
            return context.SourceType.IsGenericType &&
                  (context.SourceType.GetGenericTypeDefinition() == typeof(Dictionary<,>)) &&
                  DictionaryHasUseableTypes(context);
        }

        private static bool DictionaryHasUseableTypes(IMemberMappingContext context)
        {
            var keyAndValueTypes = context.SourceType.GetGenericArguments();

            if (keyAndValueTypes[0] != typeof(string))
            {
                return false;
            }

            if (context.TargetMember.IsEnumerable)
            {
                return (keyAndValueTypes[1] == typeof(object)) || keyAndValueTypes[1].IsEnumerable();
            }

            return context
                .MapperContext
                .ValueConverters
                .CanConvert(keyAndValueTypes[1], context.TargetMember.Type);
        }

        public IDataSource Create(IMemberMappingContext context)
            => new DictionaryDataSource(context);

        private class DictionaryDataSource : DataSourceBase
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

            public DictionaryDataSource(IMemberMappingContext context)
                : this(
                      context,
                      Expression.Variable(
                          context.SourceType.GetGenericArguments().Last(),
                          context.TargetMember.Name.ToCamelCase()))
            {
            }

            private DictionaryDataSource(IMemberMappingContext context, ParameterExpression variable)
                : base(context.SourceMember, Enumerable.Empty<Expression>(), new[] { variable }, GetValueParsing(variable, context))
            {
            }

            private static Expression GetValueParsing(Expression variable, IMemberMappingContext context)
            {
                var potentialNamesArray = Expression
                    .NewArrayInit(typeof(string), GetPotentialNames(context));

                var linqIntersect = Expression.Call(
                    _linqIntersectMethod,
                    Expression.Property(context.SourceObject, "Keys"),
                    potentialNamesArray,
                    CaseInsensitiveStringComparer.InstanceMember);

                var intersectionFirstOrDefault = Expression.Call(_linqFirstOrDefaultMethod, linqIntersect);
                var emptyString = Expression.Field(null, typeof(string), "Empty");
                var matchingNameOrEmptyString = Expression.Coalesce(intersectionFirstOrDefault, emptyString);

                var tryGetValueCall = Expression.Call(
                    context.SourceObject,
                    context.SourceObject.Type.GetMethod("TryGetValue", Constants.PublicInstance),
                    matchingNameOrEmptyString,
                    variable);

                var dictionaryValueOrDefault = Expression.Condition(
                    tryGetValueCall,
                    GetValue(variable, context),
                    GetDefaultValue(context));

                return dictionaryValueOrDefault;
            }

            private static IEnumerable<Expression> GetPotentialNames(IMemberMappingContext context)
            {
                var alternateNames = context
                    .TargetMember
                    .MemberChain
                    .Skip(1)
                    .Select(context.MapperContext.NamingSettings.GetAlternateNamesFor)
                    .CartesianProduct();

                var flattenedNameSet = (context.TargetMember.MemberChain.Count() == 2)
                    ? alternateNames.SelectMany(names => names)
                    : alternateNames.ToArray().SelectMany(context.MapperContext.NamingSettings.GetJoinedNamesFor);

                return flattenedNameSet.Select(Expression.Constant);
            }

            private static Expression GetValue(Expression variable, IMemberMappingContext context)
            {
                if (context.TargetMember.IsSimple)
                {
                    return context
                        .MapperContext
                        .ValueConverters
                        .GetConversion(variable, context.TargetMember.Type);
                }

                return context.GetMapCall(variable, 0);
            }

            private static Expression GetDefaultValue(IMemberMappingContext context)
            {
                return context
                    .MappingContext
                    .RuleSet
                    .FallbackDataSourceFactory
                    .Create(context)
                    .Value;
            }
        }
    }
}