namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using Extensions;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif

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

            var context = new MappingCreationContext(mappingData);

            context.MappingExpressions.AddUnlessNullOrEmpty(derivedTypeMappings);

            AddPopulationsAndCallbacks(context);

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

        private void AddPopulationsAndCallbacks(MappingCreationContext context)
        {
            context.MappingExpressions.AddUnlessNullOrEmpty(context.PreMappingCallback);
            context.MappingExpressions.AddRange(GetNonNullObjectPopulation(context));
            context.MappingExpressions.AddRange(GetConfiguredToTargetDataSourceMappings(context));
            context.MappingExpressions.AddUnlessNullOrEmpty(context.PostMappingCallback);
        }

        private IEnumerable<Expression> GetNonNullObjectPopulation(MappingCreationContext context)
            => GetObjectPopulation(context).WhereNotNull();

        protected abstract IEnumerable<Expression> GetObjectPopulation(MappingCreationContext context);

        private IEnumerable<Expression> GetConfiguredToTargetDataSourceMappings(MappingCreationContext context)
        {
            if (!HasConfiguredToTargetDataSources(context.MapperData, out var configuredToTargetDataSources))
            {
                yield break;
            }

            for (var i = 0; i < configuredToTargetDataSources.Count;)
            {
                var configuredToTargetDataSource = configuredToTargetDataSources[i++];
                var newSourceContext = context.WithDataSource(configuredToTargetDataSource);

                AddPopulationsAndCallbacks(newSourceContext);

                if (newSourceContext.MappingExpressions.None())
                {
                    continue;
                }

                context.UpdateFrom(newSourceContext);

                var mapping = newSourceContext.MappingExpressions.HasOne()
                    ? newSourceContext.MappingExpressions.First()
                    : Expression.Block(newSourceContext.MappingExpressions);

                mapping = MappingFactory.UseLocalToTargetDataSourceVariableIfAppropriate(
                    context.MapperData,
                    newSourceContext.MapperData,
                    configuredToTargetDataSource.Value,
                    mapping);

                if (!configuredToTargetDataSource.IsConditional)
                {
                    yield return mapping;
                    continue;
                }

                if (context.MapperData.TargetMember.IsComplex || (i > 1))
                {
                    yield return Expression.IfThen(configuredToTargetDataSource.Condition, mapping);
                    continue;
                }

                var fallback = context.MapperData.LocalVariable.Type.GetEmptyInstanceCreation(
                    context.TargetMember.ElementType,
                    context.MapperData.EnumerablePopulationBuilder.TargetTypeHelper);

                var assignFallback = context.MapperData.LocalVariable.AssignTo(fallback);

                yield return Expression.IfThenElse(configuredToTargetDataSource.Condition, mapping, assignFallback);
            }
        }

        protected static bool HasConfiguredToTargetDataSources(IMemberMapperData mapperData, out IList<IConfiguredDataSource> dataSources)
        {
            dataSources = mapperData
                .MapperContext
                .UserConfigurations
                .GetDataSourcesForToTarget(mapperData);

            return dataSources.Any();
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

        private static Expression WrapInTryCatch(Expression mappingBlock, IMemberMapperData mapperData)
        {
            var configuredCallback = mapperData.MapperContext.UserConfigurations.GetExceptionCallbackOrNull(mapperData);
            var exceptionVariable = Parameters.Create<Exception>("ex");

            if (configuredCallback == null)
            {
                var catchBody = Expression.Throw(
                    MappingException.GetFactoryMethodCall(mapperData, exceptionVariable),
                    mappingBlock.Type);

                return CreateTryCatch(mappingBlock, exceptionVariable, catchBody);
            }

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

            var createExceptionContextCall = Expression.Call(
                exceptionContextCreateMethod,
                contextAccess,
                exceptionVariable);

            var callbackInvocation = Expression.Invoke(configuredCallback, createExceptionContextCall);
            var returnDefault = mappingBlock.Type.ToDefaultExpression();
            var configuredCatchBody = Expression.Block(callbackInvocation, returnDefault);

            return CreateTryCatch(mappingBlock, exceptionVariable, configuredCatchBody);
        }

        private static Expression CreateTryCatch(
            Expression mappingBlock,
            ParameterExpression exceptionVariable,
            Expression catchBody)
        {
            var catchBlock = Expression.Catch(exceptionVariable, catchBody);

            return Expression.TryCatch(mappingBlock, catchBlock);
        }
    }
}