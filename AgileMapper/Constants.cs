namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal static class Constants
    {
        public static readonly bool ReflectionNotPermitted = ReflectionExtensions.ReflectionNotPermitted;

        public static readonly string[] EmptyStringArray = Enumerable<string>.EmptyArray;

        public static readonly string EnumerableElementName = "[i]";

        public static readonly Type[] NoTypeArguments = Enumerable<Type>.EmptyArray;

        public static readonly Expression EmptyExpression = Expression.Empty();

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
    }
}
