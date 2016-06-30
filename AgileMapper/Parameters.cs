namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
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
        public static readonly ParameterExpression TargetMember = Create<QualifiedMember>("targetMember");

        public static readonly ParameterExpression EnumerableIndex = Create<int>("i");
        public static readonly ParameterExpression EnumerableIndexNullable = Create<int?>("i");

        public static ParameterExpression Create<T>(string name = null) => Create(typeof(T), name);

        public static ParameterExpression Create(Type type) => Create(type, type.GetShortVariableName());

        public static ParameterExpression Create(Type type, string name)
            => Expression.Parameter(type, name ?? type.GetShortVariableName());

        #region Parameter Swapping

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapNothing = (lambda, context) => lambda.Body;

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForContextParameter = (lambda, context) =>
        {
            var contextType = lambda.Parameters[0].Type;

            if (contextType.IsAssignableFrom(context.Parameter.Type))
            {
                return lambda.ReplaceParameterWith(context.Parameter);
            }

            var contextTypes = contextType.GetGenericArguments();
            var sourceType = contextTypes[0];
            var targetType = contextTypes[1];
            var contextInfo = GetAppropriateMappingContext(sourceType, targetType, context);

            if (lambda.Body.NodeType != ExpressionType.Invoke)
            {
                var memberContextType = (contextTypes.Length == 2) ? contextType : contextType.GetInterfaces()[0];
                var sourceProperty = memberContextType.GetProperty("Source", Constants.PublicInstance);
                var targetProperty = memberContextType.GetProperty("Target", Constants.PublicInstance);
                var indexProperty = memberContextType.GetProperty("EnumerableIndex", Constants.PublicInstance);

                var replacementsByTarget = new Dictionary<Expression, Expression>(EquivalentMemberAccessComparer.Instance)
                {
                    [Expression.Property(lambda.Parameters[0], sourceProperty)] = contextInfo.SourceAccess,
                    [Expression.Property(lambda.Parameters[0], targetProperty)] = contextInfo.TargetAccess,
                    [Expression.Property(lambda.Parameters[0], indexProperty)] = contextInfo.Index
                };

                if (contextTypes.Length == 3)
                {
                    replacementsByTarget.Add(
                        Expression.Property(lambda.Parameters[0], "CreatedObject"),
                        contextInfo.InstanceVariable);
                }

                return lambda.Body.Replace(replacementsByTarget);
            }

            if (contextTypes.Length == 2)
            {
                var mappingContextCreateCall = Expression.Call(
                    null,
                    TypedMemberMappingContext.CreateMethod.MakeGenericMethod(contextTypes),
                    contextInfo.SourceAccess,
                    contextInfo.TargetAccess,
                    contextInfo.Index);

                return lambda.ReplaceParameterWith(mappingContextCreateCall);
            }

            var objectCreationContextCreateCall = Expression.Call(
                null,
                ObjectCreationContext.CreateMethod.MakeGenericMethod(contextTypes),
                contextInfo.SourceAccess,
                contextInfo.TargetAccess,
                contextInfo.InstanceVariable,
                contextInfo.Index);

            return lambda.ReplaceParameterWith(objectCreationContextCreateCall);
        };

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForSourceAndTarget = (lambda, context) =>
        {
            var contextInfo = GetAppropriateMappingContext(lambda, context);
            return lambda.ReplaceParametersWith(contextInfo.SourceAccess, contextInfo.TargetAccess);
        };

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForSourceTargetAndIndex = (lambda, context) =>
        {
            var contextInfo = GetAppropriateMappingContext(lambda, context);
            return lambda.ReplaceParametersWith(contextInfo.SourceAccess, contextInfo.TargetAccess, contextInfo.Index);
        };

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForSourceTargetAndInstance = (lambda, context) =>
        {
            var contextInfo = GetAppropriateMappingContext(lambda, context);
            return lambda.ReplaceParametersWith(contextInfo.SourceAccess, contextInfo.TargetAccess, contextInfo.InstanceVariable);
        };

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForSourceTargetInstanceAndIndex = (lambda, context) =>
        {
            var contextInfo = GetAppropriateMappingContext(lambda, context);

            return lambda.ReplaceParametersWith(
                contextInfo.SourceAccess,
                contextInfo.TargetAccess,
                contextInfo.InstanceVariable,
                contextInfo.Index);
        };

        private static MappingContextInfo GetAppropriateMappingContext(LambdaExpression lambda, IMemberMappingContext context)
            => GetAppropriateMappingContext(lambda.Parameters[0].Type, lambda.Parameters[1].Type, context);

        private static MappingContextInfo GetAppropriateMappingContext(
            Type sourceType,
            Type targetType,
            IMemberMappingContext context)
        {
            if (TypesMatch(sourceType, targetType, context))
            {
                return new MappingContextInfo(context);
            }

            var originalContext = context;
            Expression contextAccess = context.Parameter;

            if (context.TargetMember.IsSimple)
            {
                context = context.Parent;
            }

            while (!TypesMatch(sourceType, targetType, context))
            {
                contextAccess = Expression.Property(contextAccess, "Parent");
                context = context.Parent;
            }

            return new MappingContextInfo(originalContext, contextAccess, sourceType, targetType);
        }

        private static bool TypesMatch(Type sourceType, Type targetType, IMappingData data)
            => sourceType.IsAssignableFrom(data.SourceType) && targetType.IsAssignableFrom(data.TargetType);

        private class MappingContextInfo
        {
            private static readonly MethodInfo _getSourceMethod = typeof(IObjectMappingContext).GetMethod("GetSource", Constants.PublicInstance);
            private static readonly MethodInfo _getTargetMethod = typeof(IObjectMappingContext).GetMethod("GetTarget", Constants.PublicInstance);

            public MappingContextInfo(IMemberMappingContext context)
                : this(context, context.Parameter, context.SourceType, context.TargetType)
            {
            }

            public MappingContextInfo(
                IMemberMappingContext context,
                Expression contextAccess,
                Type sourceType,
                Type targetType)
            {
                InstanceVariable = context.InstanceVariable;
                SourceAccess = GetAccess(context, contextAccess, _getSourceMethod, sourceType, context.SourceObject);
                TargetAccess = GetAccess(context, contextAccess, _getTargetMethod, targetType, context.TargetObject);
                Index = context.EnumerableIndex;
            }

            private static Expression GetAccess(
                IMemberMappingContext context,
                Expression contextAccess,
                MethodInfo accessMethod,
                Type type,
                Expression directAccessExpression)
            {
                return (contextAccess != context.Parameter)
                    ? Expression.Call(contextAccess, accessMethod.MakeGenericMethod(type))
                    : directAccessExpression;
            }

            public Expression InstanceVariable { get; }

            public Expression SourceAccess { get; }

            public Expression TargetAccess { get; }

            public Expression Index { get; }
        }

        private class EquivalentMemberAccessComparer : IEqualityComparer<Expression>
        {
            public static readonly IEqualityComparer<Expression> Instance = new EquivalentMemberAccessComparer();

            public bool Equals(Expression x, Expression y)
            {
                if (x.NodeType != y.NodeType)
                {
                    return false;
                }

                var memberAccessX = (MemberExpression)x;
                var memberAccessY = (MemberExpression)y;

                return memberAccessX.Member == memberAccessY.Member;
            }

            public int GetHashCode(Expression obj) => 0;
        }

        #endregion
    }
}