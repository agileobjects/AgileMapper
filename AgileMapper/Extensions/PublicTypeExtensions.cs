namespace AgileObjects.AgileMapper.Extensions
{
    using System;
    using System.Reflection;
    using Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    /// <summary>
    /// Provides mapping-related extension methods for Types. These methods support mapping, and are
    /// not intended to be used from your code.
    /// </summary>
    public static class PublicTypeExtensions
    {
        internal static readonly MethodInfo IsSimpleMethod = typeof(PublicTypeExtensions)
            .GetPublicStaticMethod(nameof(IsSimple));

        /// <summary>
        /// Determines if this <paramref name="type"/> is simple.
        /// </summary>
        /// <param name="type">The Type for which to make the determination.</param>
        /// <returns>True if this <paramref name="type"/> is simple otherwise false.</returns>
        public static bool IsSimple(this Type type)
        {
            type = type.GetNonNullableType();

            if (type.GetTypeCode() != NetStandardTypeCode.Object)
            {
                return true;
            }

            if ((type == typeof(Guid)) ||
                (type == typeof(TimeSpan)) ||
                (type == typeof(DateTimeOffset)) ||
                (type == typeof(ValueType)))
            {
                return true;
            }

            return type.IsValueType() && type.IsFromBcl();
        }
    }
}
