namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
#if NETSTANDARD1_0 || NETSTANDARD1_3
    using System.Linq;
#endif
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
#if FEATURE_ASSEMBLY_TRUST
                if (typeof(ReflectionExtensions).GetAssembly().IsFullyTrusted)
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

        public static bool HasKeyAttribute(this MemberInfo memberInfo)
        {
            return memberInfo
                .GetCustomAttributes(inherit: false)
                .Any(attribute => attribute.GetType().Name == "KeyAttribute");
        }

        public static IEnumerable<Type> QueryTypes(this Assembly assembly)
        {
            try
            {
                IList<Type> types = assembly.GetAllTypes();

                if (ReflectionNotPermitted)
                {
                    types = types.FilterToArray(t => t.IsPublic());
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
