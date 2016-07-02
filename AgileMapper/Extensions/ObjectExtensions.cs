namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    internal static class ObjectExtensions
    {
        public static readonly MethodInfo GetRuntimeSourceTypeMethod =
            typeof(ObjectExtensions).GetMethod("GetRuntimeSourceType", Constants.PublicStatic);

        public static Type GetRuntimeSourceType<TDeclared>(this TDeclared item)
        {
            return !EqualityComparer<TDeclared>.Default.Equals(item, default(TDeclared)) &&
                   typeof(TDeclared).CouldHaveADifferentRuntimeType()
                    ? item.GetType()
                    : typeof(TDeclared);
        }

        public static Type GetRuntimeTargetType<TDeclared>(this TDeclared item, Type sourceType)
            => (item != null) ? item.GetType() : typeof(TDeclared).GetRuntimeTargetType(sourceType);
    }
}
