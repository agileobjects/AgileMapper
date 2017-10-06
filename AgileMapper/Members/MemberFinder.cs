namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using Caching;
    using Extensions;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    internal class MemberFinder
    {
        private readonly ICache<TypeKey, Member> _idMemberCache;
        private readonly ICache<TypeKey, IList<Member>> _membersCache;

        public MemberFinder(CacheSet cacheSet)
        {
            _idMemberCache = cacheSet.CreateScoped<TypeKey, Member>();
            _membersCache = cacheSet.CreateScoped<TypeKey, IList<Member>>();
        }

        public Member GetIdentifierOrNull(TypeKey typeIdKey)
        {
            return _idMemberCache.GetOrAdd(typeIdKey, key =>
            {
                var typeMembers = GetSourceMembers(key.Type);

                return typeMembers.FirstOrDefault(member => member.IsIdentifier);
            });
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

                return GetMembers(fields, properties, methods);
            });
        }

        public IList<Member> GetTargetMembers(Type targetType)
        {
            return _membersCache.GetOrAdd(TypeKey.ForTargetMembers(targetType), key =>
            {
                var fields = GetFields(key.Type, All);
                var properties = GetProperties(key.Type, All);
                var methods = GetMethods(key.Type, OnlyCallableSetters, Member.SetMethod);

                var constructorParameterNames = key.Type
                    .GetConstructors()
                    .SelectMany(ctor => ctor.GetParameters().Select(p => p.Name))
                    .Distinct()
                    .ToArray();

                var fieldsAndProperties = fields
                    .Concat(properties)
                    .Where(m => !constructorParameterNames.Contains(m.Name, StringComparer.OrdinalIgnoreCase))
                    .ToArray();

                return GetMembers(fieldsAndProperties, methods);
            });
        }

        #region Fields

        private static IEnumerable<Member> GetFields(Type targetType, Func<FieldInfo, bool> filter)
        {
            return targetType
                .GetPublicInstanceFields()
                .Where(filter)
                .Select(Member.Field);
        }

        private static bool All(FieldInfo field) => true;

        private static bool OnlyTargets(FieldInfo field)
            => !field.IsInitOnly || IsUseableReadOnlyTarget(field.FieldType);

        #endregion

        #region Properties

        private static IEnumerable<Member> GetProperties(Type targetType, Func<PropertyInfo, bool> filter)
        {
            return targetType
                .GetPublicInstanceProperties()
                .Where(filter)
                .Select(Member.Property);
        }

        private static bool All(PropertyInfo property) => true;

        private static bool OnlyGettable(PropertyInfo property) => property.IsReadable();

        private static bool OnlyTargets(PropertyInfo property)
        {
            if (!property.IsReadable())
            {
                // TODO: Test coverage: set-only properties
                // Ignore set-only properties:
                return false;
            }

            return property.IsWriteable() || IsUseableReadOnlyTarget(property.PropertyType);
        }

        private static bool IsUseableReadOnlyTarget(Type memberType)
        {
            if (memberType.IsArray || memberType.IsValueType() || memberType.IsNullableType())
            {
                return false;
            }

            if (memberType.IsGenericType() &&
               (memberType.GetGenericTypeDefinition() == typeof(ReadOnlyCollection<>)))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Methods

        private static IEnumerable<Member> GetMethods(
            Type targetType,
            Func<MethodInfo, bool> filter,
            Func<MethodInfo, Member> memberFactory)
        {
            return targetType
                .GetPublicInstanceMethods()
                .Where(filter)
                .Select(memberFactory);
        }

        private static readonly string[] _methodsToIgnore = { "GetHashCode", "GetType" };

        private static bool OnlyRelevantCallable(MethodBase method)
        {
            return !method.IsSpecialName &&
                    method.Name.StartsWith("Get", StringComparison.OrdinalIgnoreCase) &&
                   (Array.IndexOf(_methodsToIgnore, method.Name) == -1) &&
                    method.GetParameters().None();
        }

        private static bool OnlyCallableSetters(MethodInfo method)
        {
            return !method.IsSpecialName &&
                    method.Name.StartsWith("Set", StringComparison.OrdinalIgnoreCase) &&
                    method.GetParameters().HasOne();
        }

        #endregion

        private static IList<Member> GetMembers(params IEnumerable<Member>[] members)
        {
            var allMembers = members
                .SelectMany(m => m)
                .ToArray();

            return allMembers;
        }
    }
}
