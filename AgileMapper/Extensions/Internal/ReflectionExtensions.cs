namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System.Reflection;
    using NetStandardPolyfills;

    internal static class ReflectionExtensions
    {
        public static readonly bool ReflectionNotPermitted;

        // This definitely get executed, but code coverage doesn't pick it up
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
    }
}
