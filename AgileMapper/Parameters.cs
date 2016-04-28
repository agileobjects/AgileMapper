namespace AgileObjects.AgileMapper
{
    using System.Linq.Expressions;
    using Members;

    internal static class Parameters
    {
        public static readonly ParameterExpression MappingContext =
            Expression.Parameter(typeof(MappingContext), "mc");

        public static readonly ParameterExpression TargetMember =
            Expression.Parameter(typeof(Member), "targetMember");

        public static readonly ParameterExpression EnumerableIndex =
            Expression.Parameter(typeof(int), "i");

        public static readonly ParameterExpression EnumerableIndexNullable =
            Expression.Parameter(typeof(int?), "i");
    }
}