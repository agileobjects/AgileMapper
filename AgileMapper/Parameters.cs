namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal static class Parameters
    {
        public static readonly ParameterExpression MappingContext = Create<MappingContext>();
        public static readonly ParameterExpression ObjectMappingContext = Create<IObjectMappingContext>();

        public static readonly ParameterExpression SourceMember = Create<IQualifiedMember>("sourceMember");
        public static readonly ParameterExpression TargetMember = Create<IQualifiedMember>("targetMember");

        public static readonly ParameterExpression EnumerableIndex = Create<int>("i");
        public static readonly ParameterExpression EnumerableIndexNullable = Create<int?>("i");

        public static ParameterExpression Create<T>(string name = null) => Create(typeof(T), name);

        public static ParameterExpression Create(Type type) => Create(type, type.GetShortVariableName());

        public static ParameterExpression Create(Type type, string name)
            => Expression.Parameter(type, name ?? type.GetShortVariableName());

        #region Parameter Swapping

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapNothing = (lambda, context) => lambda.Body;

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForContextParameter = (lambda, context) =>
            lambda.ReplaceParameterWith(context.Parameter);

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForSourceAndTarget = (lambda, context) =>
            lambda.ReplaceParametersWith(context.SourceObject, GetAppropriateTargetObject(lambda, context));

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForSourceTargetAndIndex = (lambda, context) =>
            lambda.ReplaceParametersWith(
                context.SourceObject,
                GetAppropriateTargetObject(lambda, context),
                context.EnumerableIndex);

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForSourceTargetAndInstance = (lambda, context) =>
            lambda.ReplaceParametersWith(
                context.SourceObject,
                GetAppropriateTargetObject(lambda, context),
                context.InstanceVariable);

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForSourceTargetInstanceAndIndex = (lambda, context) =>
            lambda.ReplaceParametersWith(
                context.SourceObject,
                GetAppropriateTargetObject(lambda, context),
                context.InstanceVariable,
                context.EnumerableIndex);

        private static readonly MethodInfo _getInstanceMethod = typeof(IObjectMappingContext).GetMethod("GetInstance");

        private static Expression GetAppropriateTargetObject(LambdaExpression lambda, IMemberMappingContext context)
        {
            var targetParameter = lambda.Parameters.ElementAt(1);

            if (targetParameter.Type.IsAssignableFrom(context.ExistingObject.Type))
            {
                return context.InstanceVariable;
            }

            Expression contextAccess = context.Parameter;

            if (context.TargetMember.IsSimple)
            {
                context = context.Parent;
            }

            while (!targetParameter.Type.IsAssignableFrom(context.ExistingObject.Type))
            {
                contextAccess = Expression.Property(contextAccess, "Parent");
                context = context.Parent;
            }

            var instanceVariable = Expression.Call(
                contextAccess,
                _getInstanceMethod.MakeGenericMethod(context.ExistingObject.Type));

            return instanceVariable;
        }

        #endregion
    }
}