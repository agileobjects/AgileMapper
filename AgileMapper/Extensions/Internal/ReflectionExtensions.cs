namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using NetStandardPolyfills;

    internal static class ReflectionExtensions
    {
        public static readonly bool ReflectionNotPermitted;

        // This definitely gets executed, but code coverage doesn't pick it up
        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        static ReflectionExtensions()
        {
            try
            {
#if !NET_STANDARD
                if (typeof(ReflectionExtensions).Assembly.IsFullyTrusted)
                {
                    return;
                }
#endif
                typeof(TrustTester)
                    .GetNonPublicStaticMethod("IsReflectionPermitted")
                    .Invoke(null, null);
            }
            catch
            {
                ReflectionNotPermitted = true;
            }
        }

        public static bool IsReadable(this PropertyInfo property) => property.GetGetter() != null;

        public static bool IsWriteable(this PropertyInfo property) => property.GetSetter() != null;

        public static bool HasAttribute<TAttribute>(this MemberInfo memberInfo)
        {
#if NET_STANDARD
            return memberInfo
                .CustomAttributes
                .Any(a => a.AttributeType == typeof(TAttribute));
#else
            return memberInfo
                .GetCustomAttributes(typeof(TAttribute), inherit: false)
                .Any();
#endif
        }

        public static bool HasKeyAttribute(this MemberInfo memberInfo)
        {
#if NET_STANDARD
            return memberInfo
                .CustomAttributes
                .Any(a => a.AttributeType.Name == "KeyAttribute");
#else
            return memberInfo
                .GetCustomAttributes(inherit: false)
                .Any(attribute => attribute.GetType().Name == "KeyAttribute");
#endif
        }

        public static IEnumerable<Type> QueryTypes(this Assembly assembly)
        {
            try
            {
                IEnumerable<Type> types = assembly.GetAllTypes();

                if (ReflectionNotPermitted)
                {
                    types = types.Filter(t => t.IsPublic());
                }

                return types;
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.WhereNotNull();
            }
        }
    }
}
