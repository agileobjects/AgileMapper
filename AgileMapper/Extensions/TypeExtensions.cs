namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Members;

    internal static class TypeExtensions
    {
        public static bool CouldHaveADifferentRuntimeType(this Type type)
        {
            return !type.IsValueType &&
                !type.IsArray &&
                !type.IsSealed &&
                (type != typeof(string));
        }

        public static bool IsEnumerable(this Type type)
        {
            return type.IsArray ||
                (type != typeof(string) &&
                typeof(IEnumerable).IsAssignableFrom(type));
        }

        public static bool IsComplex(this Type type)
        {
            return !type.IsSimple() && !type.IsEnumerable();
        }

        public static bool IsSimple(this Type type)
        {
            return type.IsValueType || (type == typeof(string));
        }

        public static bool CanBeNull(this Type type)
        {
            return type.IsClass || type.IsInterface || type.IsNullableType();
        }

        public static bool IsNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static Type GetEnumerableElementType(this Type enumerableType)
        {
            return enumerableType.IsArray
                ? enumerableType.GetElementType()
                : enumerableType.IsGenericType
                    ? enumerableType.GetGenericArguments().First()
                    : typeof(object);
        }

        public static Type GetTargetVariableType(this Type targetType)
        {
            if (!targetType.IsEnumerable())
            {
                return targetType;
            }

            var targetElementType = targetType.GetEnumerableElementType();
            var listType = typeof(List<>).MakeGenericType(targetElementType);

            if (targetType.IsArray || listType.IsAssignableFrom(targetType))
            {
                return listType;
            }

            return typeof(ICollection<>).MakeGenericType(targetElementType);
        }

        public static Member CreateElementMember(this Type elementType)
        {
            return new Member(MemberType.EnumerableElement, "[i]", elementType);
        }
    }
}
