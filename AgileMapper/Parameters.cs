namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal static class Parameters
    {
        public static readonly ParameterExpression MappingContext = Create<MappingContext>();
        public static readonly ParameterExpression ObjectMappingContext = Create<IObjectMappingContext>();

        public static readonly ParameterExpression SourceMember = Create<QualifiedMember>("sourceMember");
        public static readonly ParameterExpression TargetMember = Create<QualifiedMember>("targetMember");

        public static readonly ParameterExpression EnumerableIndex = Create<int>("i");
        public static readonly ParameterExpression EnumerableIndexNullable = Create<int?>("i");

        public static ParameterExpression Create<T>(string name = null) => Create(typeof(T), name);

        public static ParameterExpression Create(Type type) => Create(type, type.GetShortVariableName());

        public static ParameterExpression Create(Type type, string name)
            => Expression.Parameter(type, name ?? type.GetShortVariableName());

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapNothing = (lambda, context) => lambda.Body;

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForContextParameter =
            (lambda, context) => lambda.ReplaceParameterWith(context.Parameter);

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForSourceAndTarget =
            (lambda, context) => lambda.ReplaceParametersWith(context.SourceObject, context.ExistingObject);

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForSourceTargetAndIndex =
            (lambda, context) => lambda.ReplaceParametersWith(context.SourceObject, context.ExistingObject, context.EnumerableIndex);
    }
}