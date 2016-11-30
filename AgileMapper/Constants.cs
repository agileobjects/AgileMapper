namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using NetStandardPolyfills;

    internal static class Constants
    {
        public static readonly bool ReflectionPermissionDenied;

        public static readonly string[] EmptyStringArray = Enumerable<string>.EmptyArray;

        public static readonly string EnumerableElementName = "[i]";

        public static readonly Type[] NoTypeArguments = Enumerable<Type>.EmptyArray;

        public static readonly Expression EmptyExpression = Expression.Empty();

#if !NET_STANDARD
        public static readonly BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        public static readonly BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        public static readonly BindingFlags PublicStatic = BindingFlags.Public | BindingFlags.Static;

        public static readonly BindingFlags NonPublicStatic = BindingFlags.NonPublic | BindingFlags.Static;
#endif
        public const string CreateNew = "CreateNew";

        public const string Merge = "Merge";

        public const string Overwrite = "Overwrite";

        #region Numeric Types

        public static readonly Type[] WholeNumberNumericTypes =
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong)
        };

        public static readonly Type[] NumericTypes = WholeNumberNumericTypes
            .Concat(typeof(float), typeof(decimal), typeof(double))
            .ToArray();

        public static readonly IDictionary<Type, double> NumericTypeMaxValuesByType = GetValuesByType("MaxValue");
        public static readonly IDictionary<Type, double> NumericTypeMinValuesByType = GetValuesByType("MinValue");

        private static Dictionary<Type, double> GetValuesByType(string fieldName)
        {
            return NumericTypes
                .ToDictionary(t => t, t => Convert.ToDouble(t.GetField(fieldName).GetValue(null)));
        }

        #endregion

        static Constants()
        {
            try
            {
                typeof(TrustTester)
                    .GetNonPublicStaticMethod("IsReflectionPermitted")
                    .Invoke(null, null);
            }
            catch
            {
                ReflectionPermissionDenied = true;
            }
        }
    }

    internal class TrustTester
    {
        // ReSharper disable once UnusedMember.Local
        private static void IsReflectionPermitted() { }
    }
}
