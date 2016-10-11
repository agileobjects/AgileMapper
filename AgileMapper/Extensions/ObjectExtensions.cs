namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Reflection;

    internal static class ObjectExtensions
    {
        public static readonly MethodInfo GetRuntimeSourceTypeMethod =
            typeof(ObjectExtensions).GetPublicStaticMethod("GetRuntimeSourceType");

        public static Type GetRuntimeSourceType<TDeclared>(this TDeclared item)
        {
            return (item != null) ? item.GetType() : typeof(TDeclared);
        }

        public static Type GetRuntimeTargetType<TDeclared>(this TDeclared item, Type sourceType)
        {
            return (item != null)
                ? item.GetType()
                : typeof(TDeclared).IsAssignableFrom(sourceType)
                    ? sourceType
                    : typeof(TDeclared);
        }
    }
}
