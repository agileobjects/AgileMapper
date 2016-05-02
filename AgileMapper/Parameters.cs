namespace AgileObjects.AgileMapper
{
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal static class Parameters
    {
        public static readonly ParameterExpression MappingContext =
            Expression.Parameter(typeof(MappingContext), "mc");

        public static readonly ParameterExpression ObjectMappingContext =
            Expression.Parameter(typeof(IObjectMappingContext), "omc");

        public static readonly ParameterExpression SourceMember =
            Expression.Parameter(typeof(QualifiedMember), "sourceMember");

        public static readonly ParameterExpression TargetMember =
            Expression.Parameter(typeof(QualifiedMember), "targetMember");

        public static readonly ParameterExpression EnumerableIndex =
            Expression.Parameter(typeof(int), "i");

        public static readonly ParameterExpression EnumerableIndexNullable =
            Expression.Parameter(typeof(int?), "i");
    }
}