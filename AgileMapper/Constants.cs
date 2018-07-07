namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions.Internal;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class Constants
    {
        public static readonly bool ReflectionNotPermitted = ReflectionExtensions.ReflectionNotPermitted;

        public static readonly string RootMemberName = "Root";
        public static readonly string EnumerableElementName = "[i]";

        public static readonly Type[] NoTypeArguments = Enumerable<Type>.EmptyArray;
        public static readonly Type AllTypes = typeof(Constants);

        public static readonly Expression EmptyExpression = Expression.Empty();

        public const string CreateNew = "CreateNew";

        public const string Merge = "Merge";

        public const string Overwrite = "Overwrite";

        public const string Project = "Project";

        public const int BeforeLoopExitCheck = 0;

        public const int AfterLoopExitCheck = 1;

        #region Numeric Types

        public static readonly Type[] UnsignedTypes =
        {
            typeof(byte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong)
        };

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
            .Append(typeof(float), typeof(decimal), typeof(double));

        public static readonly IDictionary<Type, double> NumericTypeMaxValuesByType = GetValuesByType("MaxValue");
        public static readonly IDictionary<Type, double> NumericTypeMinValuesByType = GetValuesByType("MinValue");

        private static Dictionary<Type, double> GetValuesByType(string fieldName)
        {
            return NumericTypes
                .ToDictionary(t => t, t => Convert.ToDouble(t.GetPublicStaticField(fieldName).GetValue(null)));
        }

        #endregion
    }
}
