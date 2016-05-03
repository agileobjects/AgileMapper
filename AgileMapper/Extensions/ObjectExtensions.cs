namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Collections.Generic;

    internal static class ObjectExtensions
    {
        public static Type GetRuntimeType<TDeclared>(this TDeclared item)
        {
            return !EqualityComparer<TDeclared>.Default.Equals(item, default(TDeclared)) &&
                   typeof(TDeclared).CouldHaveADifferentRuntimeType()
                ? item.GetType()
                : typeof(TDeclared);
        }
    }
}
