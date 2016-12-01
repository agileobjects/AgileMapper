namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Caching;
    using Extensions;
    using NetStandardPolyfills;

    internal class MemberFinder
    {
        private readonly ICache<TypeKey, Member> _idMemberCache;
        private readonly ICache<TypeKey, IEnumerable<Member>> _membersCache;

        public MemberFinder()
        {
            _idMemberCache = GlobalContext.Instance.Cache.CreateScoped<TypeKey, Member>();
            _membersCache = GlobalContext.Instance.Cache.CreateScoped<TypeKey, IEnumerable<Member>>();
        }

        public Member GetIdentifierOrNull(Type type) => GetIdentifierOrNull(TypeKey.ForTypeId(type));

        public Member GetIdentifierOrNull(TypeKey typeIdKey)
        {
            return _idMemberCache.GetOrAdd(typeIdKey, key =>
            {
                var typeMembers = GetSourceMembers(key.Type);

                return typeMembers.FirstOrDefault(member => member.IsIdentifier);
            });
        }

        public IEnumerable<Member> GetSourceMembers(Type sourceType)
        {
            return _membersCache.GetOrAdd(TypeKey.ForSourceMembers(sourceType), key =>
            {
                if (key.Type.IsEnumerable())
                {
                    return new[] { CreateElementMember(key.Type) };
                }

                var fields = GetFields(key.Type, All);
                var properties = GetProperties(key.Type, OnlyGettable);
                var methods = GetMethods(key.Type, OnlyRelevantCallable, Member.GetMethod);

                return GetMembers(fields, properties, methods);
            });
        }

        public IEnumerable<Member> GetTargetMembers(Type targetType)
        {
            return _membersCache.GetOrAdd(TypeKey.ForTargetMembers(targetType), key =>
            {
                var fields = GetFields(key.Type, OnlyTargets);
                var properties = GetProperties(key.Type, OnlyTargets);
                var methods = GetMethods(key.Type, OnlyCallableSetters, Member.SetMethod);

                return GetMembers(fields, properties, methods);
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
        {
            if (field.IsInitOnly)
            {
                // Include readonly object fields:
                return !field.FieldType.IsSimple();
            }

            return true;
        }

        #endregion

        #region Properties

        private static IEnumerable<Member> GetProperties(Type targetType, Func<PropertyInfo, bool> filter)
        {
            return targetType
                .GetPublicInstanceProperties()
                .Where(filter)
                .Select(Member.Property);
        }

        private static bool OnlyGettable(PropertyInfo property) => property.IsReadable();

        private static bool OnlyTargets(PropertyInfo property)
        {
            if (!property.IsReadable())
            {
                // Ignore set-only properties:
                return false;
            }

            if (property.IsWriteable())
            {
                return true;
            }

            // Include readonly object type properties:
            return !property.PropertyType.IsSimple();
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
                   _methodsToIgnore.DoesNotContain(method.Name) &&
                    method.GetParameters().None();
        }

        private static bool OnlyCallableSetters(MethodInfo method)
        {
            return !method.IsSpecialName &&
                    method.Name.StartsWith("Set", StringComparison.OrdinalIgnoreCase) &&
                    method.GetParameters().HasOne();
        }

        #endregion

        #region Enumerable Elements

        private static Member CreateElementMember(Type enumerableType)
        {
            return new Member(
                MemberType.EnumerableElement,
                Constants.EnumerableElementName,
                enumerableType,
                enumerableType.GetEnumerableElementType());
        }

        #endregion

        private static IEnumerable<Member> GetMembers(params IEnumerable<Member>[] members)
        {
            var allMembers = members
                .SelectMany(m => m)
                .ToArray();

            return allMembers;
        }
    }
}
