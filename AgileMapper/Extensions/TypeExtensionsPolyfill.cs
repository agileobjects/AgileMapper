namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal static class TypeExtensionsPolyfill
    {
        public static bool IsSealed(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().IsSealed;
#else
            return type.IsSealed;
#endif
        }

        public static bool IsValueType(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().IsValueType;
#else
            return type.IsValueType;
#endif
        }

        public static IEnumerable<ConstructorInfo> GetPublicInstanceConstructors(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().DeclaredConstructors.Where(c => c.IsPublic && !c.IsStatic);
#else
            return type.GetConstructors(Constants.PublicInstance);
#endif
        }

        public static IEnumerable<MemberInfo> GetPublicInstanceMember(this Type type, string name)
        {
#if NET_STANDARD
            return type.GetTypeInfo().DeclaredMembers.Where(m => m.Name == name);
#else
            return type.GetMember(name, Constants.PublicInstance);
#endif
        }

        public static IEnumerable<FieldInfo> GetPublicInstanceFields(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().DeclaredFields;
#else
            return type.GetFields(Constants.PublicInstance);
#endif
        }

        public static IEnumerable<ConstructorInfo> GetNonPublicInstanceConstructors(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsPublic && !c.IsStatic);
#else
            return type.GetConstructors(Constants.NonPublicInstance);
#endif
        }

        public static IEnumerable<PropertyInfo> GetPublicInstanceProperties(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().DeclaredProperties;
#else
            return type.GetProperties(Constants.PublicInstance);
#endif
        }

        public static PropertyInfo GetPublicInstanceProperty(this Type type, string name)
        {
#if NET_STANDARD
            return type.GetTypeInfo().GetDeclaredProperty(name);
#else
            return type.GetProperty(name, Constants.PublicInstance);
#endif
        }

        public static IEnumerable<MethodInfo> GetPublicInstanceMethods(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().DeclaredMethods.Where(m => m.IsPublic && !m.IsStatic);
#else
            return type.GetMethods(Constants.PublicInstance);
#endif
        }

        public static IEnumerable<MethodInfo> GetNonPublicInstanceMethods(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().DeclaredMethods.Where(m => !m.IsPublic && !m.IsStatic);
#else
            return type.GetMethods(Constants.NonPublicInstance);
#endif
        }

        public static IEnumerable<MethodInfo> GetPublicStaticMethods(this Type type)
        {
#if NET_STANDARD
            return type.GetTypeInfo().DeclaredMethods.Where(m => m.IsPublic && m.IsStatic);
#else
            return type.GetMethods(Constants.PublicStatic);
#endif
        }

        public static MethodInfo GetPublicStaticMethod(this Type type, string name)
        {
#if NET_STANDARD
            return type.GetPublicStaticMethods().First(m => m.Name == name);
#else
            return type.GetMethod(name, Constants.PublicStatic);
#endif
        }

        public static MethodInfo GetNonPublicStaticMethod(this Type type, string name)
        {
#if NET_STANDARD
            return type.GetTypeInfo().DeclaredMethods.First(m => !m.IsPublic && m.IsStatic && (m.Name == name));
#else
            return type.GetMethod(name, Constants.NonPublicStatic);
#endif
        }
    }
}
