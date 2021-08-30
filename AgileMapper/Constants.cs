namespace AgileObjects.AgileMapper
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching.Dictionaries;
    using Extensions.Internal;

    internal static class Constants
    {
        public static readonly bool ReflectionNotPermitted = ReflectionExtensions.ReflectionNotPermitted;

        public const string RootMemberName = "Root";
        public const string EnumerableElementName = "[i]";

        public static readonly Type[] EmptyTypeArray = Enumerable<Type>.EmptyArray;
        public static readonly ParameterExpression[] EmptyParameters = Enumerable<ParameterExpression>.EmptyArray;
        public static readonly Type AllTypes = typeof(object);

        public static readonly Expression EmptyExpression = Expression.Empty();

        public static readonly Expression NullObject = typeof(object).ToDefaultExpression();

        public static readonly ParameterExpression ExecutionContextParameter =
            Expression.Parameter(typeof(IMappingExecutionContext), "context");

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

        public static readonly Type[] NumericTypes =
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(decimal),
            typeof(double)
        };

        public static readonly ISimpleDictionary<Type, double> NumericTypeMaxValuesByType =
            new FixedSizeSimpleDictionary<Type, double>(NumericTypes.Length)
                .Add(typeof(byte), byte.MaxValue)
                .Add(typeof(sbyte), sbyte.MaxValue)
                .Add(typeof(short), short.MaxValue)
                .Add(typeof(ushort), ushort.MaxValue)
                .Add(typeof(int), int.MaxValue)
                .Add(typeof(uint), uint.MaxValue)
                .Add(typeof(long), long.MaxValue)
                .Add(typeof(ulong), ulong.MaxValue)
                .Add(typeof(float), float.MaxValue)
                .Add(typeof(decimal), (double)decimal.MaxValue)
                .Add(typeof(double), double.MaxValue);

        public static readonly ISimpleDictionary<Type, double> NumericTypeMinValuesByType =
            new FixedSizeSimpleDictionary<Type, double>(NumericTypes.Length)
                .Add(typeof(byte), byte.MinValue)
                .Add(typeof(sbyte), sbyte.MinValue)
                .Add(typeof(short), short.MinValue)
                .Add(typeof(ushort), ushort.MinValue)
                .Add(typeof(int), int.MinValue)
                .Add(typeof(uint), uint.MinValue)
                .Add(typeof(long), long.MinValue)
                .Add(typeof(ulong), ulong.MinValue)
                .Add(typeof(float), float.MinValue)
                .Add(typeof(decimal), (double)decimal.MinValue)
                .Add(typeof(double), double.MinValue);

        #endregion
    }
}
