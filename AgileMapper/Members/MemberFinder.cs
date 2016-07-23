namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Caching;
    using Extensions;

    internal class MemberFinder
    {
        private readonly ICache _globalCache;

        public MemberFinder(ICache globalCache)
        {
            _globalCache = globalCache;
        }

        public Member GetIdentifierOrNull(Type type)
        {
            return _globalCache.GetOrAdd(new TypeIdentifierKey(type), k =>
            {
                var typeMembers = GetReadableMembers(type);

                return typeMembers.FirstOrDefault(member => member.IsIdentifier);
            });
        }

        public IEnumerable<Member> GetReadableMembers(Type sourceType)
        {
            return _globalCache.GetOrAdd(sourceType.FullName + ": SourceMembers", k =>
            {
                if (sourceType.IsEnumerable())
                {
                    return new[] { sourceType.CreateElementMember() };
                }

                var fields = GetFields(sourceType, All);
                var properties = GetProperties(sourceType, OnlyGettable);
                var methods = GetMethods(sourceType, OnlyRelevantCallable, Member.GetMethod);

                return GetMembers(fields, properties, methods);
            });
        }

        public IEnumerable<Member> GetWriteableMembers(Type targetType)
        {
            return _globalCache.GetOrAdd(targetType.FullName + ": TargetMembers", k =>
            {
                var fields = GetFields(targetType, OnlyWriteable);
                var properties = GetProperties(targetType, OnlySettable);
                var methods = GetMethods(targetType, OnlySettable, Member.SetMethod);

                return GetMembers(fields, properties, methods);
            });
        }

        #region Fields

        private static IEnumerable<Member> GetFields(Type targetType, Func<FieldInfo, bool> filter)
        {
            return targetType
                .GetFields(Constants.PublicInstance)
                .Where(filter)
                .Select(Member.Field);
        }

        private static bool All(FieldInfo field)
        {
            return true;
        }

        private static bool OnlyWriteable(FieldInfo field)
        {
            return !field.IsInitOnly;
        }

        #endregion

        #region Properties

        private static IEnumerable<Member> GetProperties(Type targetType, Func<PropertyInfo, bool> filter)
        {
            return targetType
                .GetProperties(Constants.PublicInstance)
                .Where(filter)
                .Where(p => p.GetGetMethod(nonPublic: false) != null)
                .Select(Member.Property);
        }

        private static bool OnlyGettable(PropertyInfo property)
        {
            return property.GetGetMethod(nonPublic: false) != null;
        }

        private static bool OnlySettable(PropertyInfo property)
        {
            return property.GetSetMethod(nonPublic: false) != null;
        }

        #endregion

        #region Methods

        private static IEnumerable<Member> GetMethods(
            Type targetType,
            Func<MethodInfo, bool> filter,
            Func<MethodInfo, Member> memberFactory)
        {
            return targetType
                .GetMethods(Constants.PublicInstance)
                .Where(filter)
                .Select(memberFactory);
        }

        private static readonly string[] _methodsToIgnore = { "GetHashCode", "GetType" };

        private static bool OnlyRelevantCallable(MethodBase method)
        {
            return _methodsToIgnore.DoesNotContain(method.Name) &&
                method.Name.StartsWith("Get", StringComparison.OrdinalIgnoreCase) &&
                !method.Name.StartsWith("get_", StringComparison.Ordinal) &&
                method.GetParameters().None();
        }

        private static bool OnlySettable(MethodInfo method)
        {
            return
                method.Name.StartsWith("Set", StringComparison.OrdinalIgnoreCase) &&
                !method.Name.StartsWith("set_", StringComparison.Ordinal) &&
                method.GetParameters().HasOne();
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
