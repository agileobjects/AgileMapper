namespace AgileObjects.AgileMapper
{
    using System;
    using System.Collections.Generic;
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
            var contextParameter = lambda.Parameters[0];
            var contextType = contextParameter.Type;

            if (contextType.IsAssignableFrom(context.Parameter.Type))
            {
                return lambda.ReplaceParameterWith(context.Parameter);
            }

            var contextTypes = contextType.GetGenericArguments();
            var contextInfo = GetAppropriateMappingContext(contextTypes, context);

            if (lambda.Body.NodeType != ExpressionType.Invoke)
            {
                var memberContextType = (contextTypes.Length == 2) ? contextType : contextType.GetInterfaces()[0];
                var sourceProperty = memberContextType.GetProperty("Source", Constants.PublicInstance);
                var targetProperty = memberContextType.GetProperty("Target", Constants.PublicInstance);
                var indexProperty = memberContextType.GetProperty("EnumerableIndex", Constants.PublicInstance);

                var replacementsByTarget = new Dictionary<Expression, Expression>(EquivalentMemberAccessComparer.Instance)
                {
                    [Expression.Property(contextParameter, sourceProperty)] = contextInfo.SourceAccess,
                    [Expression.Property(contextParameter, targetProperty)] = contextInfo.TargetAccess,
                    [Expression.Property(contextParameter, indexProperty)] = contextInfo.Index
                };

                if (contextTypes.Length == 3)
                {
                    replacementsByTarget.Add(
                        Expression.Property(contextParameter, "CreatedObject"),
                        contextInfo.InstanceVariable);
                }

                return lambda.Body.Replace(replacementsByTarget);
            }

            return GetInvocationContextArgument(contextInfo, lambda);
        };

        private static Expression GetInvocationContextArgument(MappingContextInfo contextInfo, LambdaExpression lambda)
        {
            if (contextInfo.ContextTypes.Length == 2)
            {
                return lambda.ReplaceParameterWith(contextInfo.MemberMappingContextAccess);
            }

            var objectCreationContextCreateCall = Expression.Call(
                null,
                ObjectCreationContext.CreateMethod.MakeGenericMethod(contextInfo.ContextTypes),
                contextInfo.MemberMappingContextAccess,
                contextInfo.InstanceVariable);

            return lambda.ReplaceParameterWith(objectCreationContextCreateCall);
        }

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForSourceAndTarget = (lambda, context) =>
            ReplaceParameters(lambda, context, c => c.SourceAccess, c => c.TargetAccess);

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForSourceTargetAndIndex = (lambda, context) =>
            ReplaceParameters(lambda, context, c => c.SourceAccess, c => c.TargetAccess, c => c.Index);

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForSourceTargetAndInstance = (lambda, context) =>
            ReplaceParameters(lambda, context, c => c.SourceAccess, c => c.TargetAccess, c => c.InstanceVariable);

        public static Func<LambdaExpression, IMemberMappingContext, Expression> SwapForSourceTargetInstanceAndIndex = (lambda, context) =>
            ReplaceParameters(lambda, context, c => c.SourceAccess, c => c.TargetAccess, c => c.InstanceVariable, c => c.Index);

        private static Expression ReplaceParameters(
            LambdaExpression lambda,
            IMemberMappingContext context,
            params Func<MappingContextInfo, Expression>[] parameterFactories)
        {
            var contextInfo = GetAppropriateMappingContext(lambda, context);
            return lambda.ReplaceParametersWith(parameterFactories.Select(f => f.Invoke(contextInfo)).ToArray());
        }

        private static MappingContextInfo GetAppropriateMappingContext(LambdaExpression lambda, IMemberMappingContext context)
            => GetAppropriateMappingContext(new[] { lambda.Parameters[0].Type, lambda.Parameters[1].Type }, context);

        private static MappingContextInfo GetAppropriateMappingContext(Type[] contextTypes, IMemberMappingContext context)
        {
            if (TypesMatch(contextTypes, context))
            {
                return new MappingContextInfo(context, contextTypes);
            }

            var originalContext = context;
            Expression contextAccess = context.Parameter;

            if (context.TargetMember.IsSimple)
            {
                context = context.Parent;
            }

            while (!TypesMatch(contextTypes, context))
            {
                contextAccess = Expression.Property(contextAccess, "Parent");
                context = context.Parent;
            }

            return new MappingContextInfo(originalContext, contextAccess, contextTypes);
        }

        private static bool TypesMatch(IList<Type> contextTypes, IMappingData data)
            => contextTypes[0].IsAssignableFrom(data.SourceType) && contextTypes[1].IsAssignableFrom(data.TargetType);

        private class MappingContextInfo
        {
            private static readonly MethodInfo _getSourceMethod = typeof(IObjectMappingContext).GetMethod("GetSource", Constants.PublicInstance);
            private static readonly MethodInfo _getTargetMethod = typeof(IObjectMappingContext).GetMethod("GetTarget", Constants.PublicInstance);
            private static readonly MethodInfo _asMmcMethod = typeof(IObjectMappingContext).GetMethod("AsMemberContext", Constants.PublicInstance);

            public MappingContextInfo(IMemberMappingContext context, Type[] contextTypes)
                : this(context, context.Parameter, contextTypes)
            {
            }

            public MappingContextInfo(
                IMemberMappingContext context,
                Expression contextAccess,
                Type[] contextTypes)
            {
                ContextTypes = contextTypes;
                InstanceVariable = context.InstanceVariable;
                SourceAccess = GetAccess(context, contextAccess, _getSourceMethod, contextTypes[0], context.SourceObject);
                TargetAccess = GetAccess(context, contextAccess, _getTargetMethod, contextTypes[1], context.TargetObject);
                Index = context.EnumerableIndex;

                if (contextAccess == context.Parameter)
                {
                    MemberMappingContextAccess = context.Parameter;
                    return;
                }

                MemberMappingContextAccess = Expression.Call(
                    contextAccess,
                    _asMmcMethod.MakeGenericMethod(contextTypes[0], contextTypes[1]));
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

            public Type[] ContextTypes { get; }

            public Expression InstanceVariable { get; }

            public Expression MemberMappingContextAccess { get; }

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