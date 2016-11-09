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
            new ParametersSwapper(3, IsSourceTargetAndCreatedObject, SwapForSourceTargetAndCreatedObject),
            new ParametersSwapper(4, IsSourceTargetCreatedObjectAndIndex, SwapForSourceTargetCreatedObjectAndIndex)
        };

        private static bool IsContext(Type[] contextTypes, Type[] funcArguments)
        {
            return Is(typeof(IMappingData<,>), contextTypes, funcArguments, IsSourceAndTarget) ||
                   Is(typeof(IObjectCreationMappingData<,,>), contextTypes, funcArguments, IsSourceTargetAndCreatedObject);
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

        private static bool IsSourceTargetAndCreatedObject(Type[] contextTypes, Type[] funcArguments)
            => IsSourceAndTarget(contextTypes, funcArguments) && funcArguments[2].IsAssignableFrom(contextTypes[2]);

        private static bool IsSourceTargetCreatedObjectAndIndex(Type[] contextTypes, Type[] funcArguments)
            => IsSourceTargetAndCreatedObject(contextTypes, funcArguments) && IsIndex(funcArguments);

        #region Swap Implementations

        private static Expression SwapNothing(SwapArgs swapArgs) => swapArgs.Lambda.Body;

        private static Expression SwapForContextParameter(SwapArgs swapArgs)
        {
            var contextParameter = swapArgs.Lambda.Parameters[0];
            var contextType = contextParameter.Type;

            if (contextType.IsAssignableFrom(swapArgs.MapperData.MappingDataObject.Type))
            {
                return swapArgs.Lambda.ReplaceParameterWith(swapArgs.MapperData.MappingDataObject);
            }

            var contextTypes = contextType.GetGenericArguments();
            var contextInfo = GetAppropriateMappingContext(contextTypes, swapArgs);

            if (swapArgs.Lambda.Body.NodeType == ExpressionType.Invoke)
            {
                return GetInvocationContextArgument(contextInfo, swapArgs.Lambda);
            }

            var memberContextType = IsCallbackContext(contextTypes) ? contextType : contextType.GetInterfaces().First();
            var sourceProperty = memberContextType.GetPublicInstanceProperty("Source");
            var targetProperty = memberContextType.GetPublicInstanceProperty("Target");
            var indexProperty = memberContextType.GetPublicInstanceProperty("EnumerableIndex");
            var parentProperty = memberContextType.GetPublicInstanceProperty("Parent");

            var replacementsByTarget = new ExpressionReplacementDictionary
            {
                [Expression.Property(contextParameter, sourceProperty)] = contextInfo.SourceAccess,
                [Expression.Property(contextParameter, targetProperty)] = contextInfo.TargetAccess,
                [Expression.Property(contextParameter, indexProperty)] = contextInfo.Index,
                [Expression.Property(contextParameter, parentProperty)] = contextInfo.Parent
            };

            if (IsObjectCreationContext(contextTypes))
            {
                replacementsByTarget.Add(
                    Expression.Property(contextParameter, "CreatedObject"),
                    contextInfo.CreatedObject);
            }

            return swapArgs.Lambda.Body.Replace(replacementsByTarget);
        }

        private static bool IsCallbackContext(ICollection<Type> contextTypes) => contextTypes.Count == 2;

        private static bool IsObjectCreationContext(ICollection<Type> contextTypes) => contextTypes.Count == 3;

        private static Expression GetInvocationContextArgument(MappingContextInfo contextInfo, LambdaExpression lambda)
        {
            if (IsCallbackContext(contextInfo.ContextTypes))
            {
                return lambda.ReplaceParameterWith(contextInfo.MappingDataAccess);
            }

            var createObjectCreationContextCall = Expression.Call(
                ObjectCreationMappingData.CreateMethod.MakeGenericMethod(contextInfo.ContextTypes),
                contextInfo.MappingDataAccess,
                contextInfo.CreatedObject);

            return lambda.ReplaceParameterWith(createObjectCreationContextCall);
        }

        private static Expression SwapForSourceAndTarget(SwapArgs swapArgs) =>
            ReplaceParameters(swapArgs, c => c.SourceAccess, c => c.TargetAccess);

        private static Expression SwapForSourceTargetAndIndex(SwapArgs swapArgs) =>
            ReplaceParameters(swapArgs, c => c.SourceAccess, c => c.TargetAccess, c => c.Index);

        private static Expression SwapForSourceTargetAndCreatedObject(SwapArgs swapArgs) =>
            ReplaceParameters(swapArgs, c => c.SourceAccess, c => c.TargetAccess, c => c.CreatedObject);

        private static Expression SwapForSourceTargetCreatedObjectAndIndex(SwapArgs swapArgs) =>
            ReplaceParameters(swapArgs, c => c.SourceAccess, c => c.TargetAccess, c => c.CreatedObject, c => c.Index);

        private static Expression ReplaceParameters(
            SwapArgs swapArgs,
            params Func<MappingContextInfo, Expression>[] parameterFactories)
        {
            var contextInfo = GetAppropriateMappingContext(
                new[] { swapArgs.Lambda.Parameters[0].Type, swapArgs.Lambda.Parameters[1].Type },
                swapArgs);

            return swapArgs.Lambda.ReplaceParametersWith(parameterFactories.Select(f => f.Invoke(contextInfo)).ToArray());
        }

        private static MappingContextInfo GetAppropriateMappingContext(Type[] contextTypes, SwapArgs swapArgs)
        {
            if (swapArgs.MapperData.TypesMatch(contextTypes))
            {
                return new MappingContextInfo(swapArgs, contextTypes);
            }

            var dataAccess = swapArgs.MapperData.GetAppropriateMappingContextAccess(contextTypes);

            return new MappingContextInfo(swapArgs, dataAccess, contextTypes);
        }

        #endregion

        #endregion

        private readonly Func<Type[], Type[], bool> _applicabilityPredicate;
        private readonly Func<SwapArgs, Expression> _parametersSwapper;

        private ParametersSwapper(
            int numberOfParameters,
            Func<Type[], Type[], bool> applicabilityPredicate,
            Func<SwapArgs, Expression> parametersSwapper)
        {
            NumberOfParameters = numberOfParameters;
            _applicabilityPredicate = applicabilityPredicate;
            _parametersSwapper = parametersSwapper;
        }

        public static ParametersSwapper For(Type[] contextTypes, Type[] funcArguments)
            => _implementations.FirstOrDefault(pso => pso.AppliesTo(contextTypes, funcArguments));

        public int NumberOfParameters { get; }

        public bool AppliesTo(Type[] contextTypes, Type[] funcArguments)
            => (funcArguments.Length == NumberOfParameters) && _applicabilityPredicate.Invoke(contextTypes, funcArguments);

        public static Expression UseTargetMember(IMemberMapperData mapperData, Expression contextAccess, Type targetType)
            => mapperData.GetTargetAccess(contextAccess, targetType);

        public static Expression UseInstanceVariable(IMemberMapperData mapperData, Expression contextAccess, Type targetType)
        {
            if (!contextAccess.Type.IsGenericType())
            {
                return UseTargetMember(mapperData, contextAccess, targetType);
            }

            var instanceVariableAccess = mapperData
                .GetAppropriateMappingContext(contextAccess.Type.GetGenericArguments())
                .InstanceVariable;

            return targetType.IsAssignableFrom(instanceVariableAccess.Type)
                ? instanceVariableAccess
                : instanceVariableAccess.GetConversionTo(targetType);
        }

        public Expression Swap(
            LambdaExpression lambda,
            IMemberMapperData mapperData,
            Func<IMemberMapperData, Expression, Type, Expression> targetValueFactory)
        {
            var swapArgs = new SwapArgs(lambda, mapperData, targetValueFactory);

            return _parametersSwapper.Invoke(swapArgs);
        }

        #region Helper Classes

        public class MappingContextInfo
        {
            public MappingContextInfo(SwapArgs swapArgs, Type[] contextTypes)
                : this(swapArgs, swapArgs.MapperData.MappingDataObject, contextTypes)
            {
            }

            public MappingContextInfo(SwapArgs swapArgs, Expression contextAccess, Type[] contextTypes)
            {
                ContextTypes = contextTypes;
                CreatedObject = swapArgs.MapperData.CreatedObject;
                SourceAccess = swapArgs.MapperData.GetSourceAccess(contextAccess, contextTypes[0]);
                TargetAccess = swapArgs.TargetValueFactory.Invoke(swapArgs.MapperData, contextAccess, contextTypes[1]);
                Index = swapArgs.MapperData.EnumerableIndex;
                Parent = swapArgs.MapperData.ParentObject;
                MappingDataAccess = swapArgs.MapperData.GetTypedContextAccess(contextAccess, contextTypes);
            }

            public Type[] ContextTypes { get; }

            public Expression CreatedObject { get; }

            public Expression MappingDataAccess { get; }

            public Expression SourceAccess { get; }

            public Expression TargetAccess { get; }

            public Expression Index { get; }

            public Expression Parent { get; }
        }

        public class SwapArgs
        {
            public SwapArgs(
                LambdaExpression lambda,
                IMemberMapperData mapperData,
                Func<IMemberMapperData, Expression, Type, Expression> targetValueFactory)
            {
                Lambda = lambda;
                MapperData = mapperData;
                TargetValueFactory = targetValueFactory;
            }

            public LambdaExpression Lambda { get; }

            public IMemberMapperData MapperData { get; }

            public Func<IMemberMapperData, Expression, Type, Expression> TargetValueFactory { get; }
        }

        #endregion
    }
}