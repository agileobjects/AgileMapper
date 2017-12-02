namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Reflection;
    using NetStandardPolyfills;

    internal static class ObjectExtensions
    {
        public static readonly MethodInfo GetRuntimeSourceTypeMethod =
            typeof(ObjectExtensions).GetPublicStaticMethod("GetRuntimeSourceType");

        public static Type GetRuntimeSourceType<TDeclared>(this TDeclared item)
        {
            return (item != null) ? item.GetType() : typeof(TDeclared);
        }

        public static Type GetRuntimeTargetType<TDeclared>(this TDeclared item, Type sourceType)
            => (item != null) ? item.GetType() : typeof(TDeclared).GetRuntimeTargetType(sourceType);

        public static Type GetRuntimeTargetType(this Type targetType, Type sourceType)
            => sourceType.IsAssignableTo(targetType) ? sourceType : targetType;
    }
}
