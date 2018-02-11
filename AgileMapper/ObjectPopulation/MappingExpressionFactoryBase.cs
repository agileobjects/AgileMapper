namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using static CallbackPosition;
    using static System.Linq.Expressions.ExpressionType;

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
                return mapperData.IsEntryPoint() ? mapperData.TargetObject : Constants.EmptyExpression;
            }

            mappingExpressions.InsertRange(0, GetShortCircuitReturns(returnNull, mappingData));

            var mappingBlock = GetMappingBlock(mappingExpressions, mappingExtras);

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

        protected virtual IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingData mappingData)
            => Enumerable<Expression>.Empty;

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
            if (expression.NodeType == Constant)
            {
                return false;
            }

            if ((expression.NodeType == Call) &&
                (IsCallTo(nameof(IObjectMappingDataUntyped.Register), expression) ||
                 IsCallTo(nameof(IObjectMappingDataUntyped.TryGet), expression)))
            {
                return false;
            }

            if (expression.NodeType == Assign)
            {
                var assignment = (BinaryExpression)expression;

                if ((assignment.Right.NodeType == Call) &&
                    IsCallTo(nameof(IObjectMappingDataUntyped.MapRecursion), assignment.Right))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsCallTo(string methodName, Expression methodCall)
            => ((MethodCallExpression)methodCall).Method.Name == methodName;

        private Expression GetMappingBlock(IList<Expression> mappingExpressions, MappingExtras mappingExtras)
        {
            var mapperData = mappingExtras.MapperData;

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
                    return GetReturnExpression(mappingExpressions[0], mappingExtras);
                }

                if (TryAdjustForUnusedLocalVariableIfApplicable(
                    mappingExpressions,
                    mappingExtras,
                    mapperData,
                    out returnExpression))
                {
                    return returnExpression;
                }
            }
            else if (TryAdjustForUnusedLocalVariableIfApplicable(
                mappingExpressions,
                mappingExtras,
                mapperData,
                out returnExpression))
            {
                return returnExpression;
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
            MappingExtras mappingExtras,
            ObjectMapperData mapperData,
            out Expression returnExpression)
        {
            if (!mapperData.Context.UseLocalVariable)
            {
                returnExpression = null;
                return false;
            }

            if (!TryGetVariableAssignment(mappingExpressions, out var localVariableAssignment))
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
                    GetReturnExpression(localVariableAssignment, mappingExtras))
                : GetReturnExpression(assignedValue, mappingExtras);

            if (mappingExpressions.HasOne())
            {
                return true;
            }

            mappingExpressions[mappingExpressions.Count - 1] = mapperData.GetReturnLabel(returnExpression);
            returnExpression = Expression.Block(mappingExpressions);
            return true;
        }

        private static bool TryGetVariableAssignment(IEnumerable<Expression> mappingExpressions, out BinaryExpression binaryExpression)
        {
            binaryExpression = mappingExpressions.FirstOrDefault(exp => exp.NodeType == Assign) as BinaryExpression;
            
            return binaryExpression != null;
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