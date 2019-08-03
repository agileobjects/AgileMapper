namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal static class Constants
    {
        public static readonly bool ReflectionNotPermitted = ReflectionExtensions.ReflectionNotPermitted;

        public static readonly string RootMemberName = "Root";
        public static readonly string EnumerableElementName = "[i]";

        public static readonly Type[] EmptyTypeArray = Enumerable<Type>.EmptyArray;
        public static readonly Type AllTypes = typeof(Constants);

        public static readonly Expression EmptyExpression = Expression.Empty();

        public const string CreateNew = nameof(CreateNew);

        public const string Merge = nameof(Merge);

        public const string Overwrite = nameof(Overwrite);

        public const string Project = nameof(Project);

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

        public static readonly IList<Type> NumericTypes = WholeNumberNumericTypes
            .Append(new[] { typeof(float), typeof(decimal), typeof(double) });

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
