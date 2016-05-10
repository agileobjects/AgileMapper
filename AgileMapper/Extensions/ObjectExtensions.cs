namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;

    internal static class ObjectExtensions
    {
        public static Type GetRuntimeSourceType<TDeclared>(this TDeclared item)
        {
            return !EqualityComparer<TDeclared>.Default.Equals(item, default(TDeclared)) &&
                   typeof(TDeclared).CouldHaveADifferentRuntimeType()
                    ? item.GetType()
                    : typeof(TDeclared);
        }

        public static Type GetRuntimeTargetType<TDeclared>(this TDeclared item, Type sourceType)
        {
            return (item != null)
                ? item.GetType()
                : typeof(TDeclared).IsAssignableFrom(sourceType) ? sourceType : typeof(TDeclared);
        }
    }
}
