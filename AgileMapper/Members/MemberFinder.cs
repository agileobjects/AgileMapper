namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Extensions;

    internal class MemberFinder
    {
        //public Member GetIdentifierOrNull(Type type)
        //{
        //    var rootMember = Member.RootSource(type);
        //    var typeMembers = GetSourceMembers(rootMember);

        //    return typeMembers.FirstOrDefault(member => member.IsIdentifier);
        //}

        public IEnumerable<Member> GetSourceMembers(Type sourceMemberType)
        {
            if (sourceMemberType.IsEnumerable())
            {
                return new[] { sourceMemberType.GetEnumerableElementType().CreateElementMember() };
            }

            var fields = GetFields(sourceMemberType, All);
            var properties = GetProperties(sourceMemberType, OnlyGettable);
            var methods = GetMethods(sourceMemberType, MemberType.GetMethod, OnlyRelevantCallable, UseReturnType);

            return GetMembers(fields, properties, methods);
        }

        public IEnumerable<Member> GetTargetMembers(Type targetType)
        {
            var fields = GetFields(targetType, OnlyWriteable);
            var properties = GetProperties(targetType, OnlySettable);
            var methods = GetMethods(targetType, MemberType.SetMethod, OnlySettable, UseFirstArgumentType);

            return GetMembers(fields, properties, methods);
        }

        #region Fields

        private static IEnumerable<Member> GetFields(Type targetType, Func<FieldInfo, bool> filter)
        {
            return targetType
                .GetFields(Constants.PublicInstance)
                .Where(filter)
                .Select(f => new Member(MemberType.Field, f.Name, f.FieldType));
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
                .Select(p => new Member(MemberType.Property, p.Name, p.PropertyType));
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
            MemberType memberType,
            Func<MethodInfo, bool> filter,
            Func<MethodInfo, Type> typeSelector)
        {
            return targetType
                .GetMethods(Constants.PublicInstance)
                .Where(filter)
                .Select(m => new Member(memberType, m.Name, typeSelector.Invoke(m)));
        }

        private static readonly string[] _methodsToIgnore = { "GetHashCode", "GetType" };

        private static bool OnlyRelevantCallable(MethodBase method)
        {
            return _methodsToIgnore.DoesNotContain(method.Name) &&
                method.Name.StartsWith("Get", StringComparison.OrdinalIgnoreCase) &&
                !method.Name.StartsWith("get_", StringComparison.Ordinal) &&
                method.GetParameters().None();
        }

        private static Type UseReturnType(MethodInfo method)
        {
            return method.ReturnType;
        }

        private static bool OnlySettable(MethodInfo method)
        {
            return
                method.Name.StartsWith("Set", StringComparison.OrdinalIgnoreCase) &&
                !method.Name.StartsWith("set_", StringComparison.Ordinal) &&
                method.GetParameters().HasOne();
        }

        private static Type UseFirstArgumentType(MethodBase method)
        {
            return method.GetParameters().First().ParameterType;
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
