namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
#if NET_STANDARD
#endif
    using Members;
    using NetStandardPolyfills;
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

            var mappingExtras = GetMappingExtras(mapperData);
            var mappingExpressions = new List<Expression>();

            mappingExpressions.AddUnlessNullOrEmpty(derivedTypeMappings);
            mappingExpressions.AddUnlessNullOrEmpty(mappingExtras.PreMappingCallback);
            mappingExpressions.AddRange(GetObjectPopulation(mappingData).WhereNotNull());
            mappingExpressions.AddUnlessNullOrEmpty(mappingExtras.PostMappingCallback);

            if (NothingIsBeingMapped(mappingExpressions, mapperData))
            {
                return mapperData.IsRoot ? mapperData.TargetObject : Constants.EmptyExpression;
            }

            mappingExpressions.InsertRange(0, GetShortCircuitReturns(returnNull, mappingData));

            var mappingBlock = GetMappingBlock(mappingExpressions, mappingExtras);

            if (mapperData.Context.UseMappingTryCatch)
            {
                mappingBlock = WrapInTryCatch(mappingBlock, mapperData);
            }

            return mappingBlock;
        }

        protected abstract bool TargetCannotBeMapped(IObjectMappingData mappingData, out Expression nullMappingBlock);

        protected abstract IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingData mappingData);

        private bool MappingAlwaysBranchesToDerivedType(IObjectMappingData mappingData, out Expression derivedTypeMappings)
        {
            derivedTypeMappings = GetDerivedTypeMappings(mappingData);

            if (derivedTypeMappings.NodeType != ExpressionType.Goto)
            {
                return false;
            }

            var returnExpression = (GotoExpression)derivedTypeMappings;
            derivedTypeMappings = returnExpression.Value;
            return true;
        }

        protected abstract Expression GetDerivedTypeMappings(IObjectMappingData mappingData);

        private static MappingExtras GetMappingExtras(ObjectMapperData mapperData)
        {
            var basicMapperData = mapperData.WithNoTargetMember();
            var preMappingCallback = basicMapperData.GetMappingCallbackOrNull(Before, mapperData);
            var postMappingCallback = basicMapperData.GetMappingCallbackOrNull(After, mapperData);
            var mapToNullCondition = GetMapToNullConditionOrNull(mapperData);

            return new MappingExtras(
                mapperData,
                preMappingCallback,
                postMappingCallback,
                mapToNullCondition);
        }

        private static Expression GetMapToNullConditionOrNull(IMemberMapperData mapperData)
            => mapperData.MapperContext.UserConfigurations.GetMapToNullConditionOrNull(mapperData);

        protected abstract IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData);

        private static bool NothingIsBeingMapped(IList<Expression> mappingExpressions, ObjectMapperData mapperData)
        {
            if (mappingExpressions.None())
            {
                return true;
            }

            if (mappingExpressions[0].NodeType != ExpressionType.Assign)
            {
                return false;
            }

            var assignedValue = ((BinaryExpression)mappingExpressions[0]).Right;

            if (assignedValue.NodeType == ExpressionType.Default)
            {
                return true;
            }

            return mappingExpressions.HasOne() &&
                  (assignedValue == mapperData.TargetObject);
        }

        private Expression GetMappingBlock(IList<Expression> mappingExpressions, MappingExtras mappingExtras)
        {
            var mapperData = mappingExtras.MapperData;

            Expression returnExpression;

            AdjustForSingleExpressionBlockIfApplicable(ref mappingExpressions);

            if (mappingExpressions[0].NodeType != ExpressionType.Block)
            {
                if (mappingExpressions[0].NodeType == ExpressionType.MemberAccess)
                {
                    return GetReturnExpression(mappingExpressions[0], mappingExtras);
                }

                if (!mapperData.Context.UseLocalVariable)
                {
                    goto CreateFullMappingBlock;
                }

                var firstAssignment = (BinaryExpression)mappingExpressions.First(exp => exp.NodeType == ExpressionType.Assign);

                if ((firstAssignment.Left.NodeType == ExpressionType.Parameter) &&
                    (mappingExpressions.Last() == firstAssignment))
                {
                    returnExpression = GetReturnExpression(firstAssignment.Right, mappingExtras);

                    if (mappingExpressions.HasOne())
                    {
                        return returnExpression;
                    }

                    mappingExpressions[mappingExpressions.Count - 1] = mapperData.GetReturnLabel(returnExpression);

                    return Expression.Block(mappingExpressions);
                }
            }

            CreateFullMappingBlock:

            returnExpression = GetReturnExpression(GetReturnValue(mapperData), mappingExtras);

            mappingExpressions.Add(mapperData.GetReturnLabel(returnExpression));

            var mappingBlock = mapperData.Context.UseLocalVariable
                ? Expression.Block(new[] { mapperData.LocalVariable }, mappingExpressions)
                : Expression.Block(mappingExpressions);

            return mappingBlock;
        }

        private static void AdjustForSingleExpressionBlockIfApplicable(ref IList<Expression> mappingExpressions)
        {
            if (!mappingExpressions.HasOne() || (mappingExpressions[0].NodeType != ExpressionType.Block))
            {
                return;
            }

            var block = (BlockExpression)mappingExpressions[0];

            if (block.Expressions.HasOne() && block.Variables.None())
            {
                mappingExpressions = new List<Expression>(block.Expressions);
            }
        }

        private static Expression GetReturnExpression(Expression returnValue, MappingExtras mappingExtras)
        {
            return (mappingExtras.MapToNullCondition != null)
                ? Expression.Condition(
                    mappingExtras.MapToNullCondition,
                    returnValue.Type.ToDefaultExpression(),
                    returnValue)
                : returnValue;
        }

        protected abstract Expression GetReturnValue(ObjectMapperData mapperData);

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
                var exceptionFactoryMethod = MappingException.FactoryMethod
                    .MakeGenericMethod(mapperData.SourceType, mapperData.TargetType);

                var mappingExceptionCreation = Expression.Call(
                    exceptionFactoryMethod,
                    mapperData.MappingDataObject,
                    exceptionVariable);

                catchBody = Expression.Throw(mappingExceptionCreation, mappingBlock.Type);
            }

            var catchBlock = Expression.Catch(exceptionVariable, catchBody);

            return Expression.TryCatch(mappingBlock, catchBlock);
        }

        public virtual void Reset()
        {
        }

        #region Helper Class

        internal class MappingExtras
        {
            public MappingExtras(
                ObjectMapperData mapperData,
                Expression preMappingCallback,
                Expression postMappingCallback,
                Expression mapToNullCondition)
            {
                MapperData = mapperData;
                PreMappingCallback = preMappingCallback;
                PostMappingCallback = postMappingCallback;
                MapToNullCondition = mapToNullCondition;
            }

            public ObjectMapperData MapperData { get; }

            public Expression PreMappingCallback { get; }

            public Expression PostMappingCallback { get; }

            public Expression MapToNullCondition { get; }
        }

        #endregion
    }
}