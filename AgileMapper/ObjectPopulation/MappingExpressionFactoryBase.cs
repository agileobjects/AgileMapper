namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using MapperKeys;
    using Members;
    using Members.Sources;
    using NetStandardPolyfills;
    using static System.Linq.Expressions.ExpressionType;
    using static CallbackPosition;

    internal abstract class MappingExpressionFactoryBase
    {
        public abstract bool IsFor(IObjectMappingData mappingData);

        public Expression Create(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (TargetCannotBeMapped(mappingData, out var nullMappingBlock))
            {
                return nullMappingBlock;
            }

            var returnNull = Expression.Return(
                mapperData.ReturnLabelTarget,
                mapperData.TargetType.ToDefaultExpression());

            if (MappingAlwaysBranchesToDerivedType(mappingData, out var derivedTypeMappings))
            {
                var shortCircuitReturns = GetShortCircuitReturns(returnNull, mappingData).ToArray();

                return shortCircuitReturns.Any()
                    ? Expression.Block(shortCircuitReturns.Append(derivedTypeMappings))
                    : derivedTypeMappings;
            }

            var context = GetCreationContext(mappingData);
            var mappingExpressions = new List<Expression>();

            mappingExpressions.AddUnlessNullOrEmpty(derivedTypeMappings);
            mappingExpressions.AddUnlessNullOrEmpty(context.PreMappingCallback);
            mappingExpressions.AddRange(GetObjectPopulation(context).WhereNotNull());
            mappingExpressions.AddRange(GetConfiguredRootSourceMemberPopulations(context));
            mappingExpressions.AddUnlessNullOrEmpty(context.PostMappingCallback);

            if (NothingIsBeingMapped(mappingExpressions, mapperData))
            {
                return mapperData.IsEntryPoint ? mapperData.TargetObject : Constants.EmptyExpression;
            }

            mappingExpressions.InsertRange(0, GetShortCircuitReturns(returnNull, mappingData));

            var mappingBlock = GetMappingBlock(mappingExpressions, context, mapperData);

            if (mapperData.Context.UseMappingTryCatch)
            {
                mappingBlock = WrapInTryCatch(mappingBlock, mapperData);
            }

            return mappingBlock;
        }

        protected virtual bool TargetCannotBeMapped(IObjectMappingData mappingData, out Expression nullMappingBlock)
        {
            nullMappingBlock = null;
            return false;
        }

        private bool MappingAlwaysBranchesToDerivedType(IObjectMappingData mappingData, out Expression derivedTypeMappings)
        {
            derivedTypeMappings = GetDerivedTypeMappings(mappingData);

            if (derivedTypeMappings.NodeType != Goto)
            {
                return false;
            }

            var returnExpression = (GotoExpression)derivedTypeMappings;
            derivedTypeMappings = returnExpression.Value;
            return true;
        }

        protected virtual Expression GetDerivedTypeMappings(IObjectMappingData mappingData) => Constants.EmptyExpression;

        protected virtual IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingData mappingData)
            => Enumerable<Expression>.Empty;

        private static MappingCreationContext GetCreationContext(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;
            var mapToNullCondition = GetMapToNullConditionOrNull(mapperData);

            if (mapperData.RuleSet.Settings.UseSingleRootMappingExpression)
            {
                return new MappingCreationContext(mappingData, mapToNullCondition);
            }

            var basicMapperData = mapperData.WithNoTargetMember();
            var preMappingCallback = basicMapperData.GetMappingCallbackOrNull(Before, mapperData);
            var postMappingCallback = basicMapperData.GetMappingCallbackOrNull(After, mapperData);

            return new MappingCreationContext(
                mappingData,
                preMappingCallback,
                postMappingCallback,
                mapToNullCondition);
        }

        private static Expression GetMapToNullConditionOrNull(IMemberMapperData mapperData)
            => mapperData.MapperContext.UserConfigurations.GetMapToNullConditionOrNull(mapperData);

        protected abstract IEnumerable<Expression> GetObjectPopulation(MappingCreationContext context);

        private IEnumerable<Expression> GetConfiguredRootSourceMemberPopulations(MappingCreationContext context)
        {
            if (!context.IsRoot ||
                !context.MapperContext.UserConfigurations.HasDataSourceFactoriesForRootTarget)
            {
                yield break;
            }

            var configuredRootTargetDataSources = context
                .MapperContext
                .UserConfigurations
                .GetDataSources(context.MapperData);

            if (configuredRootTargetDataSources.None())
            {
                yield break;
            }

            // TODO: Test coverage for mappings with variables
            foreach (var configuredRootTargetDataSource in configuredRootTargetDataSources)
            {
                var newSourceMappingData = context.MappingData.WithSource(configuredRootTargetDataSource.SourceMember);

                newSourceMappingData.MapperKey = new RootObjectMapperKey(
                    context.RuleSet,
                    newSourceMappingData.MappingTypes,
                    new FixedMembersMembersSource(
                        configuredRootTargetDataSource.SourceMember,
                        context.MapperData.TargetMember));

                newSourceMappingData.MapperData.SourceObject = configuredRootTargetDataSource.Value;
                newSourceMappingData.MapperData.TargetInstance = context.MapperData.TargetInstance;

                var newSourceContext = new MappingCreationContext(newSourceMappingData)
                {
                    InstantiateLocalVariable = false
                };

                var memberPopulations = GetObjectPopulation(newSourceContext).WhereNotNull().ToArray();

                if (memberPopulations.None())
                {
                    continue;
                }

                var mapping = memberPopulations.HasOne()
                    ? memberPopulations.First()
                    : Expression.Block(memberPopulations);

                if (configuredRootTargetDataSource.IsConditional)
                {
                    mapping = Expression.IfThen(configuredRootTargetDataSource.Condition, mapping);
                }

                yield return mapping;
            }
        }

        private static bool NothingIsBeingMapped(IList<Expression> mappingExpressions, IMemberMapperData mapperData)
        {
            mappingExpressions = mappingExpressions
                .Where(IsMemberMapping)
                .ToList();

            if (mappingExpressions.None())
            {
                return true;
            }

            if (mappingExpressions[0].NodeType != Assign)
            {
                return false;
            }

            var assignedValue = ((BinaryExpression)mappingExpressions[0]).Right;

            if (assignedValue.NodeType == Default)
            {
                return true;
            }

            if (!mappingExpressions.HasOne())
            {
                return false;
            }

            if (assignedValue == mapperData.TargetObject)
            {
                return true;
            }

            if (assignedValue.NodeType == Coalesce)
            {
                var valueCoalesce = (BinaryExpression)assignedValue;

                if ((valueCoalesce.Left == mapperData.TargetObject) &&
                    (valueCoalesce.Right.NodeType == New))
                {
                    var objectNewing = (NewExpression)valueCoalesce.Right;

                    if (objectNewing.Arguments.None() && (objectNewing.Type != typeof(object)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsMemberMapping(Expression expression)
        {
            switch (expression.NodeType)
            {
                case Constant:
                    return false;

                case Call when (
                    IsCallTo(expression, nameof(IObjectMappingDataUntyped.Register)) ||
                    IsCallTo(expression, nameof(IObjectMappingDataUntyped.TryGet))):

                    return false;

                case Assign when IsMapRecursionCall(((BinaryExpression)expression).Right):
                    return false;

                default:
                    return true;
            }
        }

        private static bool IsCallTo(Expression call, string methodName)
            => ((MethodCallExpression)call).Method.Name == methodName;

        private static bool IsMapRecursionCall(Expression expression)
        {
            return (expression.NodeType == Call) &&
                    IsCallTo(expression, nameof(IObjectMappingDataUntyped.MapRecursion));
        }

        private Expression GetMappingBlock(
            IList<Expression> mappingExpressions,
            MappingCreationContext mappingCreationContext,
            ObjectMapperData mapperData)
        {
            AdjustForSingleExpressionBlockIfApplicable(ref mappingExpressions);

            if (mapperData.UseSingleMappingExpression())
            {
                return mappingExpressions.First();
            }

            if (mappingExpressions.HasOne() && (mappingExpressions[0].NodeType == Constant))
            {
                goto CreateFullMappingBlock;
            }

            Expression returnExpression;

            if (mappingExpressions[0].NodeType != Block)
            {
                if (mappingExpressions[0].NodeType == MemberAccess)
                {
                    return GetReturnExpression(mappingExpressions[0], mappingCreationContext);
                }

                if (TryAdjustForUnusedLocalVariableIfApplicable(
                    mappingExpressions,
                    mappingCreationContext,
                    mapperData,
                    out returnExpression))
                {
                    return returnExpression;
                }
            }
            else if (TryAdjustForUnusedLocalVariableIfApplicable(
                mappingExpressions,
                mappingCreationContext,
                mapperData,
                out returnExpression))
            {
                return returnExpression;
            }

            CreateFullMappingBlock:

            returnExpression = GetReturnExpression(GetReturnValue(mapperData), mappingCreationContext);

            mappingExpressions.Add(mapperData.GetReturnLabel(returnExpression));

            var mappingBlock = mapperData.Context.UseLocalVariable
                ? Expression.Block(new[] { mapperData.LocalVariable }, mappingExpressions)
                : Expression.Block(mappingExpressions);

            return mappingBlock;
        }

        private static void AdjustForSingleExpressionBlockIfApplicable(ref IList<Expression> mappingExpressions)
        {
            if (!mappingExpressions.HasOne() || (mappingExpressions[0].NodeType != Block))
            {
                return;
            }

            var block = (BlockExpression)mappingExpressions[0];

            if (block.Expressions.HasOne() && block.Variables.None())
            {
                mappingExpressions = new List<Expression>(block.Expressions);
            }
        }

        private static bool TryAdjustForUnusedLocalVariableIfApplicable(
            IList<Expression> mappingExpressions,
            MappingCreationContext context,
            ObjectMapperData mapperData,
            out Expression returnExpression)
        {
            if (!mapperData.Context.UseLocalVariable)
            {
                returnExpression = null;
                return false;
            }

            if (!mappingExpressions.TryGetVariableAssignment(out var localVariableAssignment))
            {
                returnExpression = null;
                return false;
            }

            if ((localVariableAssignment.Left.NodeType != Parameter) ||
                (localVariableAssignment != mappingExpressions.Last()))
            {
                returnExpression = null;
                return false;
            }

            var assignedValue = localVariableAssignment.Right;

            returnExpression = (assignedValue.NodeType == Invoke)
                ? Expression.Block(
                    new[] { (ParameterExpression)localVariableAssignment.Left },
                    GetReturnExpression(localVariableAssignment, context))
                : GetReturnExpression(assignedValue, context);

            if (mappingExpressions.HasOne())
            {
                return true;
            }

            mappingExpressions[mappingExpressions.Count - 1] = mapperData.GetReturnLabel(returnExpression);
            returnExpression = Expression.Block(mappingExpressions);
            return true;
        }

        private static Expression GetReturnExpression(Expression returnValue, MappingCreationContext context)
        {
            return (context.MapToNullCondition != null)
                ? Expression.Condition(
                    context.MapToNullCondition,
                    returnValue.Type.ToDefaultExpression(),
                    returnValue)
                : returnValue;
        }

        protected virtual Expression GetReturnValue(ObjectMapperData mapperData) => mapperData.TargetInstance;

        private static Expression WrapInTryCatch(Expression mappingBlock, IMemberMapperData mapperData)
        {
            var configuredCallback = mapperData.MapperContext.UserConfigurations.GetExceptionCallbackOrNull(mapperData);
            var exceptionVariable = Parameters.Create<Exception>("ex");

            Expression catchBody;

            if (configuredCallback != null)
            {
                var callbackActionType = configuredCallback.Type.GetGenericTypeArguments()[0];

                Type[] contextTypes;
                Expression contextAccess;

                if (callbackActionType.IsGenericType())
                {
                    contextTypes = callbackActionType.GetGenericTypeArguments();
                    contextAccess = mapperData.GetAppropriateTypedMappingContextAccess(contextTypes);
                }
                else
                {
                    contextTypes = new[] { mapperData.SourceType, mapperData.TargetType };
                    contextAccess = mapperData.MappingDataObject;
                }

                var exceptionContextCreateMethod = ObjectMappingExceptionData
                    .CreateMethod
                    .MakeGenericMethod(contextTypes);

                var exceptionContextCreateCall = Expression.Call(
                    exceptionContextCreateMethod,
                    contextAccess,
                    exceptionVariable);

                var callbackInvocation = Expression.Invoke(configuredCallback, exceptionContextCreateCall);
                var returnDefault = mappingBlock.Type.ToDefaultExpression();
                catchBody = Expression.Block(callbackInvocation, returnDefault);
            }
            else
            {
                catchBody = Expression.Throw(
                    MappingException.GetFactoryMethodCall(mapperData, exceptionVariable),
                    mappingBlock.Type);
            }

            var catchBlock = Expression.Catch(exceptionVariable, catchBody);

            return Expression.TryCatch(mappingBlock, catchBlock);
        }

        public virtual void Reset()
        {
        }

        #region Helper Class

        internal class MappingCreationContext
        {
            public MappingCreationContext(IObjectMappingData mappingData, Expression mapToNullCondition = null)
                : this(mappingData, null, null, mapToNullCondition)
            {
            }

            public MappingCreationContext(
                IObjectMappingData mappingData,
                Expression preMappingCallback,
                Expression postMappingCallback,
                Expression mapToNullCondition)
            {
                MappingData = mappingData;
                PreMappingCallback = preMappingCallback;
                PostMappingCallback = postMappingCallback;
                MapToNullCondition = mapToNullCondition;
                InstantiateLocalVariable = true;
            }

            public MapperContext MapperContext => MapperData.MapperContext;

            public MappingRuleSet RuleSet => MappingData.MappingContext.RuleSet;

            public ObjectMapperData MapperData => MappingData.MapperData;

            public bool IsRoot => MappingData.IsRoot;

            public IObjectMappingData MappingData { get; }

            public Expression PreMappingCallback { get; }

            public Expression PostMappingCallback { get; }

            public Expression MapToNullCondition { get; }

            public bool InstantiateLocalVariable { get; set; }
        }

        #endregion
    }
}