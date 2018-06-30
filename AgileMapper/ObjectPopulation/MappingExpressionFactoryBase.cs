namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
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

            context.MappingExpressions.AddUnlessNullOrEmpty(derivedTypeMappings);
            context.MappingExpressions.AddUnlessNullOrEmpty(context.PreMappingCallback);
            context.MappingExpressions.AddRange(GetObjectPopulation(context).WhereNotNull());
            context.MappingExpressions.AddRange(GetConfiguredRootDataSourcePopulations(context));
            context.MappingExpressions.AddUnlessNullOrEmpty(context.PostMappingCallback);

            if (NothingIsBeingMapped(context))
            {
                return mapperData.IsEntryPoint ? mapperData.TargetObject : Constants.EmptyExpression;
            }

            context.MappingExpressions.InsertRange(0, GetShortCircuitReturns(returnNull, mappingData));

            var mappingBlock = GetMappingBlock(context);

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

        private IEnumerable<Expression> GetConfiguredRootDataSourcePopulations(MappingCreationContext context)
        {
            if (!HasConfiguredRootDataSources(context.MapperData, out var configuredRootDataSources))
            {
                yield break;
            }

            // TODO: Test coverage for mappings with variables
            foreach (var configuredRootDataSource in configuredRootDataSources)
            {
                var newSourceContext = context.WithDataSource(configuredRootDataSource);

                newSourceContext.InstantiateLocalVariable = false;

                var memberPopulations = GetObjectPopulation(newSourceContext).WhereNotNull().ToArray();

                if (memberPopulations.None())
                {
                    continue;
                }

                if (context.MapperData.TargetMember.IsEnumerable)
                {
                    context.MapperData.LocalVariable = newSourceContext.MapperData.LocalVariable;
                    context.MapperData.EnumerablePopulationBuilder.TargetVariable = newSourceContext.MapperData.EnumerablePopulationBuilder.TargetVariable;
                }

                var mapping = memberPopulations.HasOne()
                    ? memberPopulations.First()
                    : Expression.Block(memberPopulations);

                if (!configuredRootDataSource.IsConditional)
                {
                    yield return mapping;
                    continue;
                }

                if (context.MapperData.TargetMember.IsComplex)
                {
                    yield return Expression.IfThen(configuredRootDataSource.Condition, mapping);
                    continue;
                }

                var fallback = context.MapperData.GetFallbackCollectionValue();
                var assignFallback = context.MapperData.LocalVariable.AssignTo(fallback);

                yield return Expression.IfThenElse(configuredRootDataSource.Condition, mapping, assignFallback);
            }
        }

        protected static bool HasConfiguredRootDataSources(IMemberMapperData mapperData, out IList<IConfiguredDataSource> dataSources)
        {
            if (!mapperData.IsRoot ||
                !mapperData.MapperContext.UserConfigurations.HasConfiguredRootDataSources)
            {
                dataSources = null;
                return false;
            }

            dataSources = mapperData
                .MapperContext
                .UserConfigurations
                .GetDataSources(mapperData)
                .ToArray();

            return dataSources.Any();
        }

        private static bool NothingIsBeingMapped(MappingCreationContext context)
        {
            var mappingExpressions = context
                .MappingExpressions
                .Filter(IsMemberMapping)
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

            if (assignedValue == context.MapperData.TargetObject)
            {
                return true;
            }

            if (assignedValue.NodeType == Coalesce)
            {
                var valueCoalesce = (BinaryExpression)assignedValue;

                if ((valueCoalesce.Left == context.MapperData.TargetObject) &&
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

        private Expression GetMappingBlock(MappingCreationContext context)
        {
            var mappingExpressions = context.MappingExpressions;

            AdjustForSingleExpressionBlockIfApplicable(ref mappingExpressions);

            if (context.MapperData.UseSingleMappingExpression())
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
                    return GetReturnExpression(mappingExpressions[0], context);
                }

                if (TryAdjustForUnusedLocalVariableIfApplicable(context, out returnExpression))
                {
                    return returnExpression;
                }
            }
            else if (TryAdjustForUnusedLocalVariableIfApplicable(context, out returnExpression))
            {
                return returnExpression;
            }

            CreateFullMappingBlock:

            returnExpression = GetReturnExpression(GetReturnValue(context.MapperData), context);

            mappingExpressions.Add(context.MapperData.GetReturnLabel(returnExpression));

            var mappingBlock = context.MapperData.Context.UseLocalVariable
                ? Expression.Block(new[] { context.MapperData.LocalVariable }, mappingExpressions)
                : Expression.Block(mappingExpressions);

            return mappingBlock;
        }

        private static void AdjustForSingleExpressionBlockIfApplicable(ref List<Expression> mappingExpressions)
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

        private static bool TryAdjustForUnusedLocalVariableIfApplicable(MappingCreationContext context, out Expression returnExpression)
        {
            if (!context.MapperData.Context.UseLocalVariable)
            {
                returnExpression = null;
                return false;
            }

            if (!context.MappingExpressions.TryGetVariableAssignment(out var localVariableAssignment))
            {
                returnExpression = null;
                return false;
            }

            if ((localVariableAssignment.Left.NodeType != Parameter) ||
                (localVariableAssignment != context.MappingExpressions.Last()))
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

            if (context.MappingExpressions.HasOne())
            {
                return true;
            }

            context.MappingExpressions[context.MappingExpressions.Count - 1] = context.MapperData.GetReturnLabel(returnExpression);
            returnExpression = Expression.Block(context.MappingExpressions);
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
            public MappingCreationContext(
                IObjectMappingData mappingData,
                Expression mapToNullCondition = null,
                List<Expression> mappingExpressions = null)
                : this(mappingData, null, null, mapToNullCondition, mappingExpressions)
            {
            }

            public MappingCreationContext(
                IObjectMappingData mappingData,
                Expression preMappingCallback,
                Expression postMappingCallback,
                Expression mapToNullCondition,
                List<Expression> mappingExpressions = null)
            {
                MappingData = mappingData;
                PreMappingCallback = preMappingCallback;
                PostMappingCallback = postMappingCallback;
                MapToNullCondition = mapToNullCondition;
                InstantiateLocalVariable = true;
                MappingExpressions = mappingExpressions ?? new List<Expression>();
            }

            public MapperContext MapperContext => MapperData.MapperContext;

            public MappingRuleSet RuleSet => MappingData.MappingContext.RuleSet;

            public ObjectMapperData MapperData => MappingData.MapperData;

            public bool IsRoot => MappingData.IsRoot;

            public IObjectMappingData MappingData { get; }

            public Expression PreMappingCallback { get; }

            public Expression PostMappingCallback { get; }

            public Expression MapToNullCondition { get; }

            public List<Expression> MappingExpressions { get; }

            public bool InstantiateLocalVariable { get; set; }

            public MappingCreationContext WithDataSource(IDataSource newDataSource)
            {
                var newSourceMappingData = MappingData.WithSource(newDataSource.SourceMember);

                newSourceMappingData.MapperKey = new RootObjectMapperKey(
                    RuleSet,
                    newSourceMappingData.MappingTypes,
                    new FixedMembersMembersSource(newDataSource.SourceMember, MapperData.TargetMember));

                newSourceMappingData.MapperData.SourceObject = newDataSource.Value;
                newSourceMappingData.MapperData.TargetObject = MapperData.TargetObject;

                if (MapperData.TargetMember.IsComplex)
                {
                    newSourceMappingData.MapperData.TargetInstance = MapperData.TargetInstance;
                }

                return new MappingCreationContext(newSourceMappingData, mappingExpressions: MappingExpressions);
            }
        }

        #endregion
    }
}