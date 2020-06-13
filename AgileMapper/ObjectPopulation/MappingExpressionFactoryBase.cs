namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ComplexTypes.ShortCircuits;
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
    using ReadableExpressions;
    using ReadableExpressions.Extensions;
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
                    return context.MappingExpressions.HasOne()
                        ? context.MappingExpressions.First()
                        : Expression.Block(context.MappingExpressions);
                }

                goto CompleteMappingBlock;
            }

            AddPopulationsAndCallbacks(context);

            if (NothingIsBeingMapped(context))
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
                }
                else
                {
                    if (isConditional)
                    {
                        context.MappingExpressions.Add(mapping);
                        continue;
                    }

                    AddPopulationsAndCallbacks(
                        mapping,
                        context,
                        (m, ctx) => ctx.MappingExpressions.Add(m));
                }

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
            isConditional = false;

            return GetConfiguredToTargetDataSourceMappings(context, sequential: false)
                .FirstOrDefault();
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
                context.MappingExpressions.AddRange(factory.GetObjectPopulation(context));

                context.MappingExpressions.AddRange(
                    GetConfiguredToTargetDataSourceMappings(context, sequential: true));
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

        protected abstract IEnumerable<Expression> GetObjectPopulation(MappingCreationContext context);

        protected IEnumerable<Expression> GetConfiguredToTargetDataSourceMappings(
            MappingCreationContext context,
            bool sequential)
        {
            if (context.MapperData.Context.IsForToTargetMapping)
            {
                yield break;
            }

            var toTargetDataSources = context
                .ToTargetDataSources
                .FilterToArray(sequential, (seq, cds) => cds.IsSequential == seq);

            if (toTargetDataSources.None())
            {
                yield break;
            }

            for (var i = 0; i < toTargetDataSources.Count; ++i)
            {
                var toTargetDataSource = toTargetDataSources[i];
                var toTargetContext = context.WithToTargetDataSource(toTargetDataSource);

                AddPopulationsAndCallbacks(toTargetContext);

                if (toTargetContext.MappingExpressions.None())
                {
                    continue;
                }

                context.UpdateFrom(toTargetContext, toTargetDataSource);

                var mapperData = context.MapperData;

                var mapping = toTargetContext.MappingExpressions.HasOne()
                    ? toTargetContext.MappingExpressions.First()
                    : Expression.Block(toTargetContext.MappingExpressions);

                mapping = MappingFactory.UseLocalToTargetDataSourceVariableIfAppropriate(
                    mapperData,
                    toTargetContext.MapperData,
                    toTargetDataSource.Value,
                    mapping);

                if ((sequential && !toTargetDataSource.IsConditional) ||
                   (!sequential && !toTargetDataSource.HasConfiguredCondition))
                {
                    yield return mapping;
                    break;
                }

                Expression fallback;

                if (mapperData.TargetMember.IsComplex || (i > 0))
                {
                    if (sequential || !mapperData.TargetMemberIsEnumerableElement())
                    {
                        yield return Expression.IfThen(toTargetDataSource.Condition, mapping);
                        continue;
                    }

                    fallback = mapperData.GetTargetMemberDefault();
                }
                else
                {
                    fallback = mapperData.LocalVariable.Type.GetEmptyInstanceCreation(
                        context.TargetMember.ElementType,
                        mapperData.EnumerablePopulationBuilder.TargetTypeHelper);
                }

                var assignFallback = mapperData.LocalVariable.AssignTo(fallback);

                yield return Expression.IfThenElse(toTargetDataSource.Condition, mapping, assignFallback);
            }
        }

        private static bool NothingIsBeingMapped(MappingCreationContext context)
        {
            var mappingExpressions = context.GetMemberMappingExpressions();

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

            returnExpression = GetReturnExpression(GetReturnValue(context.MapperData), context);

            mappingExpressions.Add(context.MapperData.GetReturnLabel(returnExpression));

            var mappingBlock = context.MapperData.Context.UseLocalVariable
                ? Expression.Block(new[] { context.MapperData.LocalVariable }, mappingExpressions)
                : Expression.Block(mappingExpressions);

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