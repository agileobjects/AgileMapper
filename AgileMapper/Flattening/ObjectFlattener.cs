namespace AgileObjects.AgileMapper.Flattening
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Caching;
    using Extensions;
    using Members;

    internal class ObjectFlattener
    {
        #region Cached Items

        private static readonly ParameterExpression _parameter = Parameters.Create<ObjectFlattener>();

        private static readonly MethodInfo _getPropertyValuesByNameMethod = typeof(ObjectFlattener)
            .GetMethods(Constants.NonPublicInstance)
            .Last(m => m.Name == "GetPropertyValuesByName");

        #endregion

        private readonly MemberFinder _memberFinder;
        private readonly ICache _cache;

        public ObjectFlattener(GlobalContext globalContext)
        {
            _memberFinder = globalContext.MemberFinder;
            _cache = globalContext.Cache;
        }

        public FlattenedObject Flatten<TSource>(TSource source)
            => new FlattenedObject(GetPropertyValuesByName(source));

        private Dictionary<string, object> GetPropertyValuesByName<TSource>(TSource source)
        {
            var propertyValuesByName =
                GetPropertyValuesByName(source, parentMemberName: null)
                    .ToDictionary(kvp => kvp.Item1, kvp => kvp.Item2);

            return propertyValuesByName;
        }

        internal IEnumerable<Tuple<string, object>> GetPropertyValuesByName(
            object parentObject,
            string parentMemberName)
        {
            foreach (var sourceMember in _memberFinder.GetReadableMembers(parentObject.GetType()))
            {
                var name = GetName(parentMemberName, sourceMember);
                var value = GetValue(parentObject, sourceMember);

                yield return Tuple.Create(name, value);

                if (sourceMember.IsComplex)
                {
                    foreach (var nestedPropertyValueByName in GetPropertyValuesByName(value, name))
                    {
                        yield return nestedPropertyValueByName;
                    }
                }
            }
        }

        private static string GetName(string parentMemberName, Member sourceMember)
        {
            if (parentMemberName == null)
            {
                return sourceMember.Name;
            }

            return parentMemberName + "_" + sourceMember.Name;
        }

        private object GetValue<TSource>(TSource source, Member member)
        {
            var sourceParameter = Parameters.Create<TSource>("source");
            var valueAccess = member.GetAccess(sourceParameter);

            if (member.Type.IsValueType)
            {
                valueAccess = valueAccess.GetConversionTo(typeof(object));
            }

            var valueLambda = Expression.Lambda<Func<TSource, object>>(valueAccess, sourceParameter);
            var valueFunc = valueLambda.Compile();

            return valueFunc.Invoke(source);
        }
    }
}