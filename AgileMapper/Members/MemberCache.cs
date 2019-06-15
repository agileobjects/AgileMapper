namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Caching;
    using Extensions;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
    using static System.StringComparer;

    internal class MemberCache
    {
        private readonly ICache<TypeKey, IList<Member>> _membersCache;

        public MemberCache(CacheSet cacheSet)
        {
            _membersCache = cacheSet.CreateScoped<TypeKey, IList<Member>>(default(HashCodeComparer<TypeKey>));
        }

        public IList<Member> GetSourceMembers(Type sourceType)
        {
            return _membersCache.GetOrAdd(TypeKey.ForSourceMembers(sourceType), key =>
            {
                if (key.Type.IsEnumerable())
                {
                    return new[] { Member.EnumerableElement(key.Type) };
                }

                var fields = GetFields(key.Type, All);
                var properties = GetProperties(key.Type, OnlyGettable);
                var methods = GetMethods(key.Type, OnlyRelevantCallable, Member.GetMethod);

                var members = new[] { fields, properties, methods };

                if (!key.Type.IsInterface())
                {
                    return GetMembers(members);
                }

                var interfaceTypes = key.Type.GetAllInterfaces();

                if (interfaceTypes.Length == 0)
                {
                    return GetMembers(members);
                }

                var interfaceCount = interfaceTypes.Length;
                
                var allMembers = new IEnumerable<Member>[3 + interfaceCount];
                allMembers.CopyFrom(members);

                for (var i = 0; i < interfaceCount; ++i)
                {
                    allMembers[i + 3] = GetSourceMembers(interfaceTypes[i]);
                }
                
                return GetMembers(allMembers);
            });
        }

        public IList<Member> GetTargetMembers(Type targetType)
        {
            return _membersCache.GetOrAdd(TypeKey.ForTargetMembers(targetType), key =>
            {
                if (key.Type.IsEnumerable())
                {
                    return Enumerable<Member>.EmptyArray;
                }

                var fields = GetFields(key.Type, All);
                var properties = GetProperties(key.Type, All);
                var methods = GetMethods(key.Type, OnlyCallableSetters, Member.SetMethod);

                var constructorParameterNames = key.Type
                    .GetPublicInstanceConstructors()
                    .SelectMany(ctor => ctor.GetParameters().ProjectToArray(p => p.Name))
                    .Distinct()
                    .ToArray();

                var fieldsAndProperties = fields
                    .Concat(properties)
                    .Project(m =>
                    {
                        m.HasMatchingCtorParameter = constructorParameterNames.Contains(m.Name, OrdinalIgnoreCase);
                        return m;
                    })
                    .ToArray();

                return GetMembers(fieldsAndProperties, methods);
            });
        }

        #region Fields

        private static IEnumerable<Member> GetFields(Type targetType, Func<FieldInfo, bool> filter)
        {
            return targetType
                .GetPublicInstanceFields()
                .Filter(filter)
                .Project(Member.Field);
        }

        private static bool All(FieldInfo field) => true;

        #endregion

        #region Properties

        private static IEnumerable<Member> GetProperties(Type targetType, Func<PropertyInfo, bool> filter)
        {
            return targetType
                .GetPublicInstanceProperties()
                .Filter(filter)
                .Project(Member.Property);
        }

        private static bool All(PropertyInfo property) => true;

        private static bool OnlyGettable(PropertyInfo property) => property.IsReadable();

        #endregion

        #region Methods

        private static IEnumerable<Member> GetMethods(
            Type targetType,
            Func<MethodInfo, bool> filter,
            Func<MethodInfo, Member> memberFactory)
        {
            return targetType
                .GetPublicInstanceMethods()
                .Filter(filter)
                .Project(memberFactory);
        }

        private static readonly string[] _methodsToIgnore = { "GetHashCode", "GetType" };

        private static bool OnlyRelevantCallable(MethodBase method)
        {
            return !method.IsSpecialName &&
                    method.Name.StartsWithIgnoreCase("Get") &&
                   (Array.IndexOf(_methodsToIgnore, method.Name) == -1) &&
                    method.GetParameters().None();
        }

        private static bool OnlyCallableSetters(MethodInfo method)
        {
            return !method.IsSpecialName &&
                    method.Name.StartsWithIgnoreCase("Set") &&
                    method.GetParameters().HasOne();
        }

        #endregion

        private static IList<Member> GetMembers(params IEnumerable<Member>[] members)
            => members.SelectMany(m => m).ToArray();
    }
}
