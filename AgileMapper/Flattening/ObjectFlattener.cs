namespace AgileObjects.AgileMapper.Flattening
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;
    using NetStandardPolyfills;

    internal class ObjectFlattener
    {
        #region Cached Items

        private static readonly ParameterExpression _objectFlattenerParameter = Parameters.Create<ObjectFlattener>();

        private static readonly MethodInfo _getPropertiesMethod = typeof(ObjectFlattener)
            .GetNonPublicInstanceMethods("GetPropertyValuesByName")
            .Last();

        #endregion

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
            foreach (var sourceMember in GlobalContext.Instance.MemberCache.GetSourceMembers(typeof(TSource)))
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
                    foreach (var nestedPropertyValueByName in GetComplexTypePropertyValuesByName(value, sourceMember.Type, name))
                    {
                        yield return nestedPropertyValueByName;
                    }

                    continue;
                }

                foreach (var nestedPropertyValueByName in GetEnumerablePropertyValuesByName(value, sourceMember.ElementType, name))
                {
                    yield return nestedPropertyValueByName;
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
                return member.Type.IsValueType() ? Activator.CreateInstance(member.Type) : null;
            }

            var cacheKey = typeof(TSource).FullName + $".{member.Name}: GetValue";

            var valueFunc = GlobalContext.Instance.Cache.GetOrAdd(cacheKey, k =>
            {
                var sourceParameter = Parameters.Create<TSource>("source");
                var valueAccess = member.GetAccess(sourceParameter);

                if (member.Type.IsValueType())
                {
                    valueAccess = valueAccess.GetConversionToObject();
                }

                var valueLambda = Expression.Lambda<Func<TSource, object>>(valueAccess, sourceParameter);

                return valueLambda.Compile();
            });

            return valueFunc.Invoke(source);
        }

        private IEnumerable<Tuple<string, object>> GetComplexTypePropertyValuesByName(
            object parentComplexType,
            Type parentMemberType,
            string parentMemberName)
        {
            var cacheKey = parentMemberType.FullName + ": GetPropertiesCaller";

            var getPropertiesFunc = GlobalContext.Instance.Cache.GetOrAdd(cacheKey, k =>
            {
                var sourceParameter = Parameters.Create<object>("source");
                var parentMemberNameParameter = Parameters.Create<string>("parentMemberName");

                var getPropertiesCall = Expression.Call(
                    _objectFlattenerParameter,
                    _getPropertiesMethod.MakeGenericMethod(parentMemberType),
                    sourceParameter.GetConversionTo(parentMemberType),
                    parentMemberNameParameter);

                var getPropertiesLambda = Expression
                    .Lambda<Func<ObjectFlattener, object, string, IEnumerable<Tuple<string, object>>>>(
                        getPropertiesCall,
                        _objectFlattenerParameter,
                        sourceParameter,
                        parentMemberNameParameter);

                return getPropertiesLambda.Compile();
            });

            return getPropertiesFunc.Invoke(this, parentComplexType, parentMemberName);
        }

        private IEnumerable<Tuple<string, object>> GetEnumerablePropertyValuesByName(
            object parentEnumerable,
            Type declaredEnumerableElementType,
            string parentMemberName)
        {
            var items = (IEnumerable)parentEnumerable;

            var i = 0;

            foreach (var item in items)
            {
                var itemType = item?.GetType() ?? declaredEnumerableElementType;

                foreach (var nestedPropertyValueByName in GetComplexTypePropertyValuesByName(item, itemType, parentMemberName + i))
                {
                    yield return nestedPropertyValueByName;
                }

                ++i;
            }
        }
    }
}