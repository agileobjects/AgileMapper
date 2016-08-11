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
        public static readonly ParameterExpression ObjectMapperData = Create<ObjectMapperData>();

        public static readonly ParameterExpression SourceMember = Create<IQualifiedMember>("sourceMember");
        public static readonly ParameterExpression TargetMember = Create<QualifiedMember>("targetMember");

        public static readonly ParameterExpression EnumerableIndex = Create<int>("i");
        public static readonly ParameterExpression EnumerableIndexNullable = Create<int?>("i");

        public static ParameterExpression Create<T>(string name = null) => Create(typeof(T), name);

        public static ParameterExpression Create(Type type) => Create(type, type.GetShortVariableName());

        public static ParameterExpression Create(Type type, string name)
            => Expression.Parameter(type, name ?? type.GetShortVariableName());

        #region Parameter Swapping

        public static Func<LambdaExpression, MemberMapperData, Expression> SwapNothing = (lambda, context) => lambda.Body;

        public static Func<LambdaExpression, MemberMapperData, Expression> SwapForContextParameter = (lambda, context) =>
        {
            var contextParameter = lambda.Parameters[0];
            var contextType = contextParameter.Type;

            if (contextType.IsAssignableFrom(context.MdParameter.Type))
            {
                return lambda.ReplaceParameterWith(context.MdParameter);
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
                return lambda.ReplaceParameterWith(contextInfo.MappingDataAccess);
            }

            var objectCreationContextCreateCall = Expression.Call(
                ObjectCreationContext.CreateMethod.MakeGenericMethod(contextInfo.ContextTypes),
                contextInfo.MappingDataAccess,
                contextInfo.InstanceVariable);

            return lambda.ReplaceParameterWith(objectCreationContextCreateCall);
        }

        public static Func<LambdaExpression, MemberMapperData, Expression> SwapForSourceAndTarget = (lambda, context) =>
            ReplaceParameters(lambda, context, c => c.SourceAccess, c => c.TargetAccess);

        public static Func<LambdaExpression, MemberMapperData, Expression> SwapForSourceTargetAndIndex = (lambda, context) =>
            ReplaceParameters(lambda, context, c => c.SourceAccess, c => c.TargetAccess, c => c.Index);

        public static Func<LambdaExpression, MemberMapperData, Expression> SwapForSourceTargetAndInstance = (lambda, context) =>
            ReplaceParameters(lambda, context, c => c.SourceAccess, c => c.TargetAccess, c => c.InstanceVariable);

        public static Func<LambdaExpression, MemberMapperData, Expression> SwapForSourceTargetInstanceAndIndex = (lambda, context) =>
            ReplaceParameters(lambda, context, c => c.SourceAccess, c => c.TargetAccess, c => c.InstanceVariable, c => c.Index);

        private static Expression ReplaceParameters(
            LambdaExpression lambda,
            MemberMapperData context,
            params Func<MappingContextInfo, Expression>[] parameterFactories)
        {
            var contextInfo = GetAppropriateMappingContext(lambda, context);
            return lambda.ReplaceParametersWith(parameterFactories.Select(f => f.Invoke(contextInfo)).ToArray());
        }

        private static MappingContextInfo GetAppropriateMappingContext(LambdaExpression lambda, MemberMapperData context)
            => GetAppropriateMappingContext(new[] { lambda.Parameters[0].Type, lambda.Parameters[1].Type }, context);

        private static MappingContextInfo GetAppropriateMappingContext(Type[] dataTypes, MemberMapperData context)
        {
            if (TypesMatch(dataTypes, context))
            {
                return new MappingContextInfo(context, dataTypes);
            }

            var originalContext = context;
            Expression dataAccess = context.MdParameter;

            if (context.TargetMember.IsSimple)
            {
                context = context.Parent;
            }

            while (!TypesMatch(dataTypes, context))
            {
                dataAccess = Expression.Property(dataAccess, "Parent");
                context = context.Parent;
            }

            return new MappingContextInfo(originalContext, dataAccess, dataTypes);
        }

        private static bool TypesMatch(IList<Type> dataTypes, BasicMapperData data)
            => dataTypes[0].IsAssignableFrom(data.SourceType) && dataTypes[1].IsAssignableFrom(data.TargetType);

        private class MappingContextInfo
        {
            private static readonly MethodInfo _getSourceMethod = typeof(IMappingData).GetMethod("GetSource", Constants.PublicInstance);
            private static readonly MethodInfo _getTargetMethod = typeof(IMappingData).GetMethod("GetTarget", Constants.PublicInstance);

            public MappingContextInfo(MemberMapperData data, Type[] contextTypes)
                : this(data, data.MdParameter, contextTypes)
            {
            }

            public MappingContextInfo(
                MemberMapperData data,
                Expression contextAccess,
                Type[] contextTypes)
            {
                ContextTypes = contextTypes;
                InstanceVariable = data.InstanceVariable;
                SourceAccess = GetAccess(data, contextAccess, _getSourceMethod, contextTypes[0], data.SourceObject);
                TargetAccess = GetAccess(data, contextAccess, _getTargetMethod, contextTypes[1], data.TargetObject);
                Index = data.EnumerableIndex;

                if (contextAccess == data.MdParameter)
                {
                    MappingDataAccess = data.MdParameter;
                    return;
                }

                MappingDataAccess = contextAccess;
            }

            private static Expression GetAccess(
                MemberMapperData data,
                Expression contextAccess,
                MethodInfo accessMethod,
                Type type,
                Expression directAccessExpression)
            {
                return (contextAccess != data.MdParameter)
                    ? Expression.Call(contextAccess, accessMethod.MakeGenericMethod(type))
                    : directAccessExpression;
            }

            public Type[] ContextTypes { get; }

            public Expression InstanceVariable { get; }

            public Expression MappingDataAccess { get; }

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