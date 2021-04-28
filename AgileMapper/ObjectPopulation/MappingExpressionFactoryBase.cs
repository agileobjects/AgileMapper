namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ComplexTypes.ShortCircuits;
    using DataSources;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Enumerables.EnumerableExtensions;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using Members.MemberExtensions;
    using NetStandardPolyfills;
    using ReadableExpressions;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

    internal abstract class MappingExpressionFactoryBase
    {
        public Expression Create(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (TargetCannotBeMapped(mappingData, out var reason))
            {
                return Expression.Block(
                    ReadableExpression.Comment(reason),
                    GetNullMappingFallbackValue(mapperData));
            }

            var context = new MappingCreationContext(mappingData);

            if (ShortCircuitMapping(context))
            {
                if (context.MappingComplete)
                {
                    return context.GetMappingExpression();
                }

                goto CompleteMappingBlock;
            }

            AddPopulationsAndCallbacks(context);

            if (context.RemoveEmptyMappings && NothingIsBeingMapped(context))
            {
                return mapperData.IsEntryPoint ? mapperData.TargetObject : Constants.EmptyExpression;
            }

        CompleteMappingBlock:
            InsertShortCircuitReturns(context);

            var mappingBlock = GetMappingBlock(context);

            if (mapperData.Context.UseMappingTryCatch)
            {
                mappingBlock = mappingBlock.WrapInTryCatch(mapperData);
            }

            return mappingBlock;
        }

        protected virtual bool TargetCannotBeMapped(IObjectMappingData mappingData, out string reason)
        {
            reason = null;
            return false;
        }

        protected virtual Expression GetNullMappingFallbackValue(IMemberMapperData mapperData)
            => mapperData.GetTargetMemberDefault();

        private bool ShortCircuitMapping(MappingCreationContext context)
        {
            foreach (var factory in AlternateMappingFactories)
            {
                var mapping = factory.Invoke(context, out var isConditional);

                if (mapping == null)
                {
                    continue;
                }

                if (context.MappingComplete)
                {
                    InsertShortCircuitReturns(context);

                    var returnLabel = context.MapperData
                        .GetFinalisedReturnValue(mapping, out var returnsDefault);

                    if (returnsDefault)
                    {
                        context.MappingExpressions.Add(mapping);
                    }

                    context.MappingExpressions.Add(returnLabel);
                    return true;
                }

                if (isConditional)
                {
                    context.MappingExpressions.Add(mapping);
                    continue;
                }

                AddPopulationsAndCallbacks(
                    mapping,
                    context,
                    (m, ctx) => ctx.MappingExpressions.Add(m));

                return true;
            }

            return false;
        }

        protected virtual IEnumerable<AlternateMappingFactory> AlternateMappingFactories
        {
            get
            {
                yield return GetConfiguredAlternateDataSourceMappingOrNull;
                yield return ConfiguredMappingFactory.GetMappingOrNull;
            }
        }

        private Expression GetConfiguredAlternateDataSourceMappingOrNull(
            MappingCreationContext context,
            out bool isConditional)
        {
            var toTargetDataSource = context
                .ToTargetDataSources
                .FirstOrDefault(ds => !ds.IsSequential && !ds.HasConfiguredMatcher);

            if (toTargetDataSource == null)
            {
                isConditional = false;
                return null;
            }

            isConditional = toTargetDataSource.HasConfiguredCondition;

            return GetConfiguredToTargetDataSourceMappingOrNull(
                context,
                toTargetDataSource,
                isFirstDataSource: true);
        }

        private void InsertShortCircuitReturns(MappingCreationContext context)
        {
            if (ShortCircuitFactories != Enumerable<ShortCircuitFactory>.EmptyArray)
            {
                context.MappingExpressions.InsertRange(0, EnumerateShortCircuitReturns(context));
            }
        }

        private IEnumerable<Expression> EnumerateShortCircuitReturns(MappingCreationContext context)
        {
            var mappingData = context.MappingData;

            foreach (var shortCircuitFactory in ShortCircuitFactories)
            {
                var shortCircuit = shortCircuitFactory.Invoke(mappingData);

                if (shortCircuit != null)
                {
                    yield return shortCircuit;
                }
            }
        }

        protected virtual IEnumerable<ShortCircuitFactory> ShortCircuitFactories
            => Enumerable<ShortCircuitFactory>.EmptyArray;

        private void AddPopulationsAndCallbacks(MappingCreationContext context)
        {
            AddPopulationsAndCallbacks(this, context, (factory, ctx) =>
            {
                var mappingExpressions = ctx.MappingExpressions;
                var mappingExpressionCount = mappingExpressions.Count;

                factory.AddObjectPopulation(ctx);
                mappingExpressions.AddRange(GetConfiguredToTargetDataSourceMappings(ctx));

                if (!context.RemoveEmptyMappings)
                {
                    return;
                }

                var addedExpressionCount = mappingExpressions.Count - mappingExpressionCount;

                if (addedExpressionCount == 0)
                {
                    return;
                }

                if (mappingExpressionCount > 0)
                {
                    mappingExpressions = mappingExpressions
                        .GetRange(mappingExpressionCount, addedExpressionCount);
                }

                if (NothingIsBeingMapped(mappingExpressions, ctx))
                {
                    ctx.MappingExpressions.RemoveRange(mappingExpressionCount, addedExpressionCount);
                }
            });
        }

        private static void AddPopulationsAndCallbacks<TArg>(
            TArg argument,
            MappingCreationContext context,
            Action<TArg, MappingCreationContext> mappingBodyPopulator)
        {
            context.MappingExpressions.AddUnlessNullOrEmpty(context.PreMappingCallback);
            mappingBodyPopulator.Invoke(argument, context);
            context.MappingExpressions.AddUnlessNullOrEmpty(context.PostMappingCallback);
        }

        protected abstract void AddObjectPopulation(MappingCreationContext context);

        private IEnumerable<Expression> GetConfiguredToTargetDataSourceMappings(
            MappingCreationContext context)
        {
            if (context.MapperData.Context.IsForToTargetMapping)
            {
                yield break;
            }

            var toTargetDataSources = context
                .ToTargetDataSources
                .Filter(cds => cds.IsSequential);

            var i = 0;

            foreach (var toTargetDataSource in toTargetDataSources)
            {
                var toTargetMapping = GetConfiguredToTargetDataSourceMappingOrNull(
                    context,
                    toTargetDataSource,
                    isFirstDataSource: i == 0);

                ++i;

                if (toTargetMapping != null)
                {
                    yield return toTargetMapping;
                }
            }
        }

        private Expression GetConfiguredToTargetDataSourceMappingOrNull(
            MappingCreationContext context,
            IConfiguredDataSource toTargetDataSource,
            bool isFirstDataSource)
        {
            if (context.MapperData.Context.IsForToTargetMapping)
            {
                return null;
            }

            var toTargetContext = context.WithToTargetDataSource(toTargetDataSource);

            AddPopulationsAndCallbacks(toTargetContext);

            if (toTargetContext.MappingExpressions.None())
            {
                return null;
            }

            context.UpdateFrom(toTargetContext, toTargetDataSource);

            var originalMapperData = context.MapperData;
            var isSequential = toTargetDataSource.IsSequential;

            if (!isSequential)
            {
                toTargetContext.MappingExpressions.Add(
                    context.MapperData.GetReturnExpression(GetExpressionToReturn(toTargetContext)));
            }

            var toTargetMapping = MappingFactory.UseLocalToTargetDataSourceVariableIfAppropriate(
                originalMapperData,
                toTargetContext.MapperData,
                toTargetDataSource.Value,
                toTargetContext.GetMappingExpression());

            var hasCondition = isSequential
                ? toTargetDataSource.IsConditional
                : toTargetDataSource.HasConfiguredCondition;

            if (!hasCondition)
            {
                return toTargetMapping;
            }

            Expression fallback;

            if (!isFirstDataSource || originalMapperData.TargetMember.IsComplex)
            {
                if (isSequential || !originalMapperData.TargetMemberIsEnumerableElement())
                {
                    return Expression.IfThen(toTargetDataSource.Condition, toTargetMapping);
                }

                // Mapping a configured ToTargetInstead() data source to
                // a complex type enumerable element member; reset the
                // local instance variable to null to prevent reuse of a
                // previous element's mapping result:
                fallback = originalMapperData.GetTargetMemberDefault();
            }
            else
            {
                fallback = originalMapperData.LocalVariable.Type.GetEmptyInstanceCreation(
                    context.TargetMember.ElementType,
                    originalMapperData.EnumerablePopulationBuilder.TargetTypeHelper);
            }

            var assignFallback = originalMapperData.LocalVariable.AssignTo(fallback);

            return Expression.IfThenElse(toTargetDataSource.Condition, toTargetMapping, assignFallback);
        }

        private static bool NothingIsBeingMapped(MappingCreationContext context)
            => NothingIsBeingMapped(context.GetMemberMappingExpressions(), context);

        private static bool NothingIsBeingMapped(
            IList<Expression> mappingExpressions,
            MappingCreationContext context)
        {
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

            if ((assignedValue.NodeType == New) &&
                 context.MapperData.TargetMemberIsEnumerableElement() &&
               ((NewExpression)assignedValue).Arguments.None())
            {
                return true;
            }

            if (assignedValue.NodeType != Coalesce)
            {
                return false;
            }

            var valueCoalesce = (BinaryExpression)assignedValue;

            if ((valueCoalesce.Left != context.MapperData.TargetObject) ||
                (valueCoalesce.Right.NodeType != New))
            {
                return false;
            }

            var objectNewing = (NewExpression)valueCoalesce.Right;

            return objectNewing.Arguments.None() && (objectNewing.Type != typeof(object));
        }

        private Expression GetMappingBlock(MappingCreationContext context)
        {
            var mappingExpressions = context.MappingExpressions;

            AdjustForSingleExpressionBlockIfApplicable(context);

            var firstExpression = mappingExpressions.First();

            if (firstExpression.NodeType == Goto)
            {
                return ((GotoExpression)firstExpression).Value;
            }

            if (context.MapperData.UseSingleMappingExpression())
            {
                return firstExpression;
            }

            Expression returnExpression;

            if (firstExpression.NodeType != Block)
            {
                if (TryAdjustForUnusedLocalVariableIfApplicable(context, out returnExpression))
                {
                    return returnExpression;
                }
            }
            else if (TryAdjustForUnusedLocalVariableIfApplicable(context, out returnExpression))
            {
                return returnExpression;
            }

            mappingExpressions.Add(context.MapperData.GetReturnLabel(GetExpressionToReturn(context)));

            var mappingBlock = context.MapperData.Context.UseLocalVariable
                ? Expression.Block(new[] { context.MapperData.LocalVariable }, mappingExpressions)
                : mappingExpressions.ToExpression();

            return mappingBlock;
        }

        private static void AdjustForSingleExpressionBlockIfApplicable(MappingCreationContext context)
        {
            if (!context.MappingExpressions.HasOne() || (context.MappingExpressions[0].NodeType != Block))
            {
                return;
            }

            var block = (BlockExpression)context.MappingExpressions[0];

            if (block.Expressions.HasOne() && block.Variables.None())
            {
                context.MappingExpressions.Clear();
                context.MappingExpressions.AddRange(block.Expressions);
            }
        }

        private static bool TryAdjustForUnusedLocalVariableIfApplicable(MappingCreationContext context, out Expression returnExpression)
        {
            if (!context.MapperData.Context.UseLocalVariable)
            {
                returnExpression = null;
                return false;
            }

            var mappingExpressions = context.MappingExpressions;

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
                    GetExpressionToReturn(localVariableAssignment, context))
                : GetExpressionToReturn(assignedValue, context);

            if (mappingExpressions.HasOne())
            {
                return true;
            }

            mappingExpressions[mappingExpressions.Count - 1] = context.MapperData.GetReturnLabel(returnExpression);
            returnExpression = Expression.Block(mappingExpressions);
            return true;
        }

        private Expression GetExpressionToReturn(MappingCreationContext context)
            => GetExpressionToReturn(GetReturnValue(context.MapperData), context);

        private static Expression GetExpressionToReturn(Expression returnValue, MappingCreationContext context)
        {
            var mapToNullCondition = GetMapToNullConditionOrNull(context);

            return (mapToNullCondition != null)
                ? Expression.Condition(
                    mapToNullCondition,
                    returnValue.Type.ToDefaultExpression(),
                    returnValue)
                : returnValue;
        }

        private static Expression GetMapToNullConditionOrNull(MappingCreationContext context)
        {
            if (context.MapperData.IsRoot || (context.MapToNullCondition != null))
            {
                return context.MapToNullCondition;
            }

            var mappingExpressions = context.GetMemberMappingExpressions();

            if (mappingExpressions.None())
            {
                return null;
            }

            var memberAssignments = mappingExpressions
                .Project(m =>
                {
                    if (m.NodeType != Conditional)
                    {
                        return m;
                    }

                    var conditionalMapping = (ConditionalExpression)m;

                    if (conditionalMapping.Test.IsNullableHasValueAccess() &&
                       (conditionalMapping.IfTrue.NodeType == Assign))
                    {
                        return conditionalMapping.IfTrue;
                    }

                    return m;
                })
                .Filter(m => m.NodeType == Assign)
                .Cast<BinaryExpression>()
                .Filter(a => a.Left.NodeType == MemberAccess)
                .ToArray();

            if (memberAssignments.Length != 1)
            {
                return null;
            }

            var assignedMember = (MemberExpression)memberAssignments[0].Left;

            var memberTypeIdentifier = context
                .MapperContext
                .GetIdentifierOrNull(assignedMember.Member.DeclaringType);

            if (!Equals(assignedMember.Member, memberTypeIdentifier?.MemberInfo))
            {
                return null;
            }

            var nonNullableIdType = assignedMember.Type.GetNonNullableType();

            if ((nonNullableIdType == assignedMember.Type) || !nonNullableIdType.IsNumeric())
            {
                return assignedMember.GetIsDefaultComparison();
            }

            return Expression.Equal(
                assignedMember.GetValueOrDefaultCall(),
                0.ToConstantExpression(nonNullableIdType));
        }

        protected virtual Expression GetReturnValue(ObjectMapperData mapperData) => mapperData.TargetInstance;
    }
}