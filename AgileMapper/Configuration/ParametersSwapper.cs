namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;
    using Members;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;

    internal class ParametersSwapper
    {
        #region Cached Items

        private static readonly ParametersSwapper[] _implementations =
        {
            new ParametersSwapper(0, (ct, ft) => true, SwapNothing),
            new ParametersSwapper(1, IsContext, SwapForContextParameter),
            new ParametersSwapper(2, IsSourceAndTarget, SwapForSourceAndTarget),
            new ParametersSwapper(3, IsSourceTargetAndIndex, SwapForSourceTargetAndIndex),
            new ParametersSwapper(3, IsSourceTargetAndInstance, SwapForSourceTargetAndInstance),
            new ParametersSwapper(4, IsSourceTargetInstanceAndIndex, SwapForSourceTargetInstanceAndIndex)
        };

        private static bool IsContext(Type[] contextTypes, Type[] funcArguments)
        {
            return Is(typeof(IMappingData<,>), contextTypes, funcArguments, IsSourceAndTarget) ||
                   Is(typeof(IObjectCreationMappingData<,,>), contextTypes, funcArguments, IsSourceTargetAndInstance);
        }

        private static bool Is(
            Type contextType,
            Type[] contextTypes,
            IList<Type> funcArguments,
            Func<Type[], Type[], bool> parametersChecker)
        {
            var contextTypeArgument = funcArguments[0];

            if (!contextTypeArgument.IsGenericType())
            {
                return false;
            }

            var contextGenericDefinition = contextTypeArgument.GetGenericTypeDefinition();

            if (contextGenericDefinition != contextType)
            {
                return false;
            }

            return parametersChecker.Invoke(contextTypes, contextTypeArgument.GetGenericArguments());
        }

        private static bool IsSourceAndTarget(Type[] contextTypes, Type[] funcArguments)
            => funcArguments[0].IsAssignableFrom(contextTypes[0]) && funcArguments[1].IsAssignableFrom(contextTypes[1]);

        private static bool IsSourceTargetAndIndex(Type[] contextTypes, Type[] funcArguments)
            => IsSourceAndTarget(contextTypes, funcArguments) && IsIndex(funcArguments);

        private static bool IsIndex(IEnumerable<Type> funcArguments) => funcArguments.Last() == typeof(int?);

        private static bool IsSourceTargetAndInstance(Type[] contextTypes, Type[] funcArguments)
            => IsSourceAndTarget(contextTypes, funcArguments) && (contextTypes.Length >= 3) && funcArguments[2].IsAssignableFrom(contextTypes[2]);

        private static bool IsSourceTargetInstanceAndIndex(Type[] contextTypes, Type[] funcArguments)
            => IsSourceTargetAndInstance(contextTypes, funcArguments) && IsIndex(funcArguments);

        #region Swap Implementations

        private static Expression SwapNothing(LambdaExpression lambda, MemberMapperData mapperData) => lambda.Body;

        private static Expression SwapForContextParameter(LambdaExpression lambda, MemberMapperData mapperData)
        {
            var contextParameter = lambda.Parameters[0];
            var contextType = contextParameter.Type;

            if (contextType.IsAssignableFrom(mapperData.Parameter.Type))
            {
                return lambda.ReplaceParameterWith(mapperData.Parameter);
            }

            var contextTypes = contextType.GetGenericArguments();
            var contextInfo = GetAppropriateMappingContext(contextTypes, mapperData);

            if (lambda.Body.NodeType != ExpressionType.Invoke)
            {
                var memberContextType = (contextTypes.Length == 2) ? contextType : contextType.GetInterfaces().First();
                var sourceProperty = memberContextType.GetPublicInstanceProperty("Source");
                var targetProperty = memberContextType.GetPublicInstanceProperty("Target");
                var indexProperty = memberContextType.GetPublicInstanceProperty("EnumerableIndex");

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
        }

        private static Expression GetInvocationContextArgument(MappingContextInfo contextInfo, LambdaExpression lambda)
        {
            if (contextInfo.ContextTypes.Length == 2)
            {
                return lambda.ReplaceParameterWith(contextInfo.MappingDataAccess);
            }

            var objectCreationContextCreateCall = Expression.Call(
                ObjectCreationMappingData.CreateMethod.MakeGenericMethod(contextInfo.ContextTypes),
                contextInfo.MappingDataAccess,
                contextInfo.InstanceVariable);

            return lambda.ReplaceParameterWith(objectCreationContextCreateCall);
        }

        private static Expression SwapForSourceAndTarget(LambdaExpression lambda, MemberMapperData mapperData) =>
            ReplaceParameters(lambda, mapperData, c => c.SourceAccess, c => c.TargetAccess);

        private static Expression SwapForSourceTargetAndIndex(LambdaExpression lambda, MemberMapperData mapperData) =>
            ReplaceParameters(lambda, mapperData, c => c.SourceAccess, c => c.TargetAccess, c => c.Index);

        private static Expression SwapForSourceTargetAndInstance(LambdaExpression lambda, MemberMapperData mapperData) =>
            ReplaceParameters(lambda, mapperData, c => c.SourceAccess, c => c.TargetAccess, c => c.InstanceVariable);

        private static Expression SwapForSourceTargetInstanceAndIndex(LambdaExpression lambda, MemberMapperData mapperData) =>
            ReplaceParameters(lambda, mapperData, c => c.SourceAccess, c => c.TargetAccess, c => c.InstanceVariable, c => c.Index);

        private static Expression ReplaceParameters(
            LambdaExpression lambda,
            MemberMapperData mapperData,
            params Func<MappingContextInfo, Expression>[] parameterFactories)
        {
            var contextInfo = GetAppropriateMappingContext(
                new[] { lambda.Parameters[0].Type, lambda.Parameters[1].Type },
                mapperData);

            return lambda.ReplaceParametersWith(parameterFactories.Select(f => f.Invoke(contextInfo)).ToArray());
        }

        private static MappingContextInfo GetAppropriateMappingContext(Type[] contextTypes, MemberMapperData mapperData)
        {
            if (mapperData.TypesMatch(contextTypes))
            {
                return new MappingContextInfo(mapperData, contextTypes);
            }

            var originalContext = mapperData;
            var dataAccess = mapperData.GetAppropriateMappingContextAccess(contextTypes);

            return new MappingContextInfo(originalContext, dataAccess, contextTypes);
        }

        #endregion

        #endregion

        private readonly int _numberOfParameters;
        private readonly Func<Type[], Type[], bool> _applicabilityPredicate;
        private readonly Func<LambdaExpression, MemberMapperData, Expression> _parametersSwapper;

        private ParametersSwapper(
            int numberOfParameters,
            Func<Type[], Type[], bool> applicabilityPredicate,
            Func<LambdaExpression, MemberMapperData, Expression> parametersSwapper)
        {
            _numberOfParameters = numberOfParameters;
            _applicabilityPredicate = applicabilityPredicate;
            _parametersSwapper = parametersSwapper;
        }

        public static ParametersSwapper For(Type[] contextTypes, Type[] funcArguments)
            => _implementations.FirstOrDefault(pso => pso.AppliesTo(contextTypes, funcArguments));

        public bool AppliesTo(Type[] contextTypes, Type[] funcArguments)
            => (funcArguments.Length == _numberOfParameters) && _applicabilityPredicate.Invoke(contextTypes, funcArguments);

        public Expression Swap(LambdaExpression lambda, MemberMapperData mapperData)
            => _parametersSwapper.Invoke(lambda, mapperData);

        #region Helper Classes

        private class MappingContextInfo
        {
            public MappingContextInfo(MemberMapperData mapperData, Type[] contextTypes)
                : this(mapperData, mapperData.Parameter, contextTypes)
            {
            }

            public MappingContextInfo(
                MemberMapperData mapperData,
                Expression contextAccess,
                Type[] contextTypes)
            {
                ContextTypes = contextTypes;
                InstanceVariable = mapperData.InstanceVariable;
                SourceAccess = mapperData.GetSourceAccess(contextAccess, contextTypes[0]);
                TargetAccess = mapperData.GetTargetAccess(contextAccess, contextTypes[1]);
                Index = mapperData.EnumerableIndex;
                MappingDataAccess = mapperData.GetTypedContextAccess(contextAccess, contextTypes);
            }

            public Type[] ContextTypes { get; }

            public Expression InstanceVariable { get; }

            public Expression MappingDataAccess { get; }

            public Expression SourceAccess { get; }

            public Expression TargetAccess { get; }

            public Expression Index { get; }
        }

        #endregion
    }
}