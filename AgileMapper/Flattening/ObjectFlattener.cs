namespace AgileObjects.AgileMapper.Flattening
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;

    internal class ObjectFlattener
    {
        #region Cached Items

        private static readonly ParameterExpression _objectFlattenerParameter = Parameters.Create<ObjectFlattener>();

        private static readonly MethodInfo _getPropertiesMethod = typeof(ObjectFlattener)
            .GetMethods(Constants.NonPublicInstance)
            .Last(m => m.Name == "GetPropertyValuesByName");

        #endregion

        private readonly MapperContext _mapperContext;

        public ObjectFlattener(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
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

        internal IEnumerable<Tuple<string, object>> GetPropertyValuesByName<TSource>(
            TSource parentObject,
            string parentMemberName)
        {
            foreach (var sourceMember in _mapperContext.GlobalContext.MemberFinder.GetReadableMembers(typeof(TSource)))
            {
                var name = GetName(parentMemberName, sourceMember);
                var value = GetValue(parentObject, sourceMember);

                if (sourceMember.IsSimple || (sourceMember.IsEnumerable && sourceMember.ElementType.IsSimple()))
                {
                    yield return Tuple.Create(name, value);
                    continue;
                }

                if (sourceMember.IsComplex)
                {
                    foreach (var nestedPropertyValueByName in GetNestedPropertyValuesByName(value, sourceMember, name))
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
            if (source == null)
            {
                return default(TSource);
            }

            var cacheKey = typeof(TSource).FullName + $".{member.Name}: GetValue";

            var valueFunc = _mapperContext.GlobalContext.Cache.GetOrAdd(cacheKey, k =>
            {
                var sourceParameter = Parameters.Create<TSource>("source");
                var valueAccess = member.GetAccess(sourceParameter);

                if (member.Type.IsValueType)
                {
                    valueAccess = valueAccess.GetConversionTo(typeof(object));
                }

                var valueLambda = Expression.Lambda<Func<TSource, object>>(valueAccess, sourceParameter);

                return valueLambda.Compile();
            });

            return valueFunc.Invoke(source);
        }

        private IEnumerable<Tuple<string, object>> GetNestedPropertyValuesByName(
            object parentObject,
            Member parentMember,
            string parentMemberName)
        {
            var cacheKey = parentMember.Type.FullName + ": GetPropertiesCaller";

            var getPropertiesFunc = _mapperContext.GlobalContext.Cache.GetOrAdd(cacheKey, k =>
            {
                var sourceParameter = Parameters.Create<object>("source");
                var parentMemberNameParameter = Parameters.Create<string>("parentMemberName");

                var getPropertiesCall = Expression.Call(
                    _objectFlattenerParameter,
                    _getPropertiesMethod.MakeGenericMethod(parentMember.Type),
                    sourceParameter.GetConversionTo(parentMember.Type),
                    parentMemberNameParameter);

                var getPropertiesLambda = Expression
                    .Lambda<Func<ObjectFlattener, object, string, IEnumerable<Tuple<string, object>>>>(
                        getPropertiesCall,
                        _objectFlattenerParameter,
                        sourceParameter,
                        parentMemberNameParameter);

                return getPropertiesLambda.Compile();
            });

            return getPropertiesFunc.Invoke(this, parentObject, parentMemberName);
        }
    }
}