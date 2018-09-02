namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using static Members.Member;

    internal class ParametersSwapper
    {
        #region Cached Items

        private static readonly ParametersSwapper[] _implementations =
        {
            new ParametersSwapper(0, (ct, ft) => true, SwapNothing),
            new ParametersSwapper(1, IsContext, SwapForContextParameter, true),
            new ParametersSwapper(1, IsSourceOnly, SwapForSource),
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

            return parametersChecker.Invoke(contextTypes, contextTypeArgument.GetGenericTypeArguments());
        }

        private static bool IsSourceOnly(Type[] contextTypes, Type[] funcArguments)
            => (contextTypes.Length == 1) && IsSource(contextTypes, funcArguments);

        private static bool IsSource(IList<Type> contextTypes, IList<Type> funcArguments)
            => contextTypes[0].IsAssignableTo(funcArguments[0]);

        private static bool IsSourceAndTarget(Type[] contextTypes, Type[] funcArguments)
            => IsSource(contextTypes, funcArguments) && contextTypes[1].IsAssignableTo(funcArguments[1]);

        private static bool IsSourceTargetAndIndex(Type[] contextTypes, Type[] funcArguments)
            => IsSourceAndTarget(contextTypes, funcArguments) && IsIndex(funcArguments);

        private static bool IsIndex(IEnumerable<Type> funcArguments) => funcArguments.Last() == typeof(int?);

        private static bool IsSourceTargetAndCreatedObject(Type[] contextTypes, Type[] funcArguments)
            => IsSourceAndTarget(contextTypes, funcArguments) && contextTypes[2].IsAssignableTo(funcArguments[2]);

        private static bool IsSourceTargetCreatedObjectAndIndex(Type[] contextTypes, Type[] funcArguments)
            => IsSourceTargetAndCreatedObject(contextTypes, funcArguments) && IsIndex(funcArguments);

        #region Swap Implementations

        private static Expression SwapNothing(SwapArgs swapArgs) => swapArgs.Lambda.Body;

        private static Expression SwapForContextParameter(SwapArgs swapArgs)
        {
            var contextParameter = swapArgs.Lambda.Parameters[0];
            var contextType = contextParameter.Type;

            if (swapArgs.MapperData.MappingDataObject.Type.IsAssignableTo(contextType))
            {
                return swapArgs.Lambda.ReplaceParameterWith(swapArgs.MapperData.MappingDataObject);
            }

            var contextTypes = contextType.GetGenericTypeArguments();
            var contextInfo = GetAppropriateMappingContext(swapArgs);

            if (swapArgs.Lambda.Body.NodeType == ExpressionType.Invoke)
            {
                return GetInvocationContextArgument(contextInfo, swapArgs.Lambda);
            }

            var memberContextType = IsCallbackContext(contextTypes) ? contextType : contextType.GetAllInterfaces().First();
            var sourceProperty = memberContextType.GetPublicInstanceProperty(RootSourceMemberName);
            var targetProperty = memberContextType.GetPublicInstanceProperty(RootTargetMemberName);
            var indexProperty = memberContextType.GetPublicInstanceProperty("EnumerableIndex");
            var parentProperty = memberContextType.GetPublicInstanceProperty("Parent");

            var replacementsByTarget = new ExpressionReplacementDictionary(5)
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

        private static Expression SwapForSource(SwapArgs swapArgs) =>
            ReplaceParameters(swapArgs, c => c.SourceAccess);

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
            var contextInfo = GetAppropriateMappingContext(swapArgs);

            return swapArgs.Lambda.ReplaceParametersWith(parameterFactories.Project(f => f.Invoke(contextInfo)).ToArray());
        }

        private static MappingContextInfo GetAppropriateMappingContext(SwapArgs swapArgs)
        {
            if (swapArgs.ContextTypesMatch())
            {
                return new MappingContextInfo(swapArgs);
            }

            var dataAccess = swapArgs.GetAppropriateMappingContextAccess();

            return new MappingContextInfo(swapArgs, dataAccess);
        }

        #endregion

        #endregion

        private readonly int _numberOfParameters;
        private readonly Func<Type[], Type[], bool> _applicabilityPredicate;
        private readonly Func<SwapArgs, Expression> _parametersSwapper;

        private ParametersSwapper(
            int numberOfParameters,
            Func<Type[], Type[], bool> applicabilityPredicate,
            Func<SwapArgs, Expression> parametersSwapper,
            bool hasMappingContextParameter = false)
        {
            _numberOfParameters = numberOfParameters;
            _applicabilityPredicate = applicabilityPredicate;
            _parametersSwapper = parametersSwapper;
            HasMappingContextParameter = hasMappingContextParameter;
        }

        public static ParametersSwapper For(Type[] contextTypes, Type[] funcArguments)
            => _implementations.FirstOrDefault(pso => pso.AppliesTo(contextTypes, funcArguments));

        public bool HasMappingContextParameter { get; }

        public bool AppliesTo(Type[] contextTypes, Type[] funcArguments)
            => (funcArguments.Length == _numberOfParameters) && _applicabilityPredicate.Invoke(contextTypes, funcArguments);

        public static Expression UseTargetMember(IMemberMapperData mapperData, Expression contextAccess, Type targetType)
            => mapperData.GetTargetAccess(contextAccess, targetType);

        public static Expression UseTargetInstance(IMemberMapperData mapperData, Expression contextAccess, Type targetType)
        {
            if (!contextAccess.Type.IsGenericType())
            {
                return UseTargetMember(mapperData, contextAccess, targetType);
            }

            var targetInstanceAccess = mapperData
                .GetAppropriateMappingContext(contextAccess.Type.GetGenericTypeArguments())
                .TargetInstance;

            return ConvertTargetType(targetType, targetInstanceAccess)
                ? targetInstanceAccess.GetConversionTo(targetType)
                : targetInstanceAccess;
        }

        private static bool ConvertTargetType(Type targetType, Expression targetInstanceAccess)
            => targetInstanceAccess.Type.IsValueType() || !targetInstanceAccess.Type.IsAssignableTo(targetType);

        public Expression Swap(
            LambdaExpression lambda,
            Type[] contextTypes,
            IMemberMapperData mapperData,
            Func<IMemberMapperData, Expression, Type, Expression> targetValueFactory)
        {
            var swapArgs = new SwapArgs(lambda, contextTypes, mapperData, targetValueFactory);

            return _parametersSwapper.Invoke(swapArgs);
        }

        #region Helper Classes

        public class MappingContextInfo
        {
            private readonly SwapArgs _swapArgs;

            public MappingContextInfo(SwapArgs swapArgs)
                : this(swapArgs, swapArgs.MapperData.MappingDataObject)
            {
            }

            public MappingContextInfo(SwapArgs swapArgs, Expression contextAccess)
            {
                _swapArgs = swapArgs;

                CreatedObject = GetCreatedObject(swapArgs);
                SourceAccess = GetValueAccess(swapArgs.GetSourceAccess(contextAccess), ContextTypes[0]);
                TargetAccess = GetValueAccess(swapArgs.GetTargetAccess(contextAccess), ContextTypes[1]);
                MappingDataAccess = swapArgs.GetTypedContextAccess(contextAccess);
            }

            private static Expression GetCreatedObject(SwapArgs swapArgs)
            {
                var neededCreatedObjectType = swapArgs.ContextTypes.Last();
                var createdObject = swapArgs.MapperData.CreatedObject;

                if ((swapArgs.ContextTypes.Length == 3) && (neededCreatedObjectType == typeof(int?)))
                {
                    return createdObject;
                }

                return GetValueAccess(createdObject, neededCreatedObjectType);
            }

            private static Expression GetValueAccess(Expression valueAccess, Type neededAccessType)
            {
                return (neededAccessType != valueAccess.Type) && valueAccess.Type.IsValueType()
                    ? valueAccess.GetConversionTo(neededAccessType)
                    : valueAccess;
            }

            public Type[] ContextTypes => _swapArgs.ContextTypes;

            public Expression CreatedObject { get; }

            public Expression MappingDataAccess { get; }

            public Expression SourceAccess { get; }

            public Expression TargetAccess { get; }

            public Expression Index => _swapArgs.MapperData.EnumerableIndex;

            public Expression Parent => _swapArgs.MapperData.ParentObject;
        }

        public class SwapArgs
        {
            public SwapArgs(
                LambdaExpression lambda,
                Type[] contextTypes,
                IMemberMapperData mapperData,
                Func<IMemberMapperData, Expression, Type, Expression> targetValueFactory)
            {
                Lambda = lambda;
                ContextTypes = (contextTypes.Length > 1) ? contextTypes : contextTypes.Append(typeof(object));
                MapperData = mapperData;
                TargetValueFactory = targetValueFactory;
            }

            public LambdaExpression Lambda { get; }

            public Type[] ContextTypes { get; }

            public IMemberMapperData MapperData { get; }

            public Func<IMemberMapperData, Expression, Type, Expression> TargetValueFactory { get; }

            public bool ContextTypesMatch() => MapperData.TypesMatch(ContextTypes);

            public Expression GetAppropriateMappingContextAccess()
                => MapperData.GetAppropriateMappingContextAccess(ContextTypes);

            public Expression GetTypedContextAccess(Expression contextAccess)
                => MapperData.GetTypedContextAccess(contextAccess, ContextTypes);

            public Expression GetSourceAccess(Expression contextAccess)
                => MapperData.GetSourceAccess(contextAccess, ContextTypes[0]);

            public Expression GetTargetAccess(Expression contextAccess)
                => TargetValueFactory.Invoke(MapperData, contextAccess, ContextTypes[1]);
        }

        #endregion
    }
}