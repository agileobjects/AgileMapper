namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Reflection;
    using NetStandardPolyfills;

    /// <summary>
    /// Provides extensions methods on the Object Type. This class is not intended 
    /// to be used from your code.
    /// </summary>
    public static class ObjectExtensions
    {
        internal static readonly MethodInfo GetRuntimeSourceTypeMethod =
            typeof(ObjectExtensions).GetPublicStaticMethod("GetRuntimeSourceType");

        /// <summary>
        /// Gets the runtime type of the given <paramref name="source"/> object.
        /// </summary>
        /// <typeparam name="TDeclared">The declared Type of the given <paramref name="source"/> object.</typeparam>
        /// <param name="source">The source object for which to determine the runtime Type.</param>
        /// <returns>The runtime type of the given <paramref name="source"/> object.</returns>
        public static Type GetRuntimeSourceType<TDeclared>(this TDeclared source)
            => source?.GetType() ?? typeof(TDeclared);

        internal static Type GetRuntimeTargetType<TDeclared>(this TDeclared item, Type sourceType)
            => (item != null) ? item.GetType() : typeof(TDeclared).GetRuntimeTargetType(sourceType);

        internal static Type GetRuntimeTargetType(this Type targetType, Type sourceType)
            => sourceType.IsAssignableTo(targetType) ? sourceType : targetType;
    }
}
