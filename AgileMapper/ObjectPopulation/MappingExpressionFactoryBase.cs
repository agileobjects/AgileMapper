namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Members;
    using NetStandardPolyfills;

    internal abstract class MappingExpressionFactoryBase
    {
        private const CallbackPosition Before = CallbackPosition.Before;
        private const CallbackPosition After = CallbackPosition.After;

        public abstract bool IsFor(IObjectMappingData mappingData);

        public Expression Create(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            Expression nullMappingBlock;

            if (TargetCannotBeMapped(mappingData, out nullMappingBlock))
            {
                return nullMappingBlock;
            }

            var returnNull = Expression.Return(
                mapperData.ReturnLabelTarget,
                mapperData.TargetType.ToDefaultExpression());

            var mappingExpressions = GetShortCircuitReturns(returnNull, mappingData).ToList();

            Expression derivedTypeMappings;

            if (MappingAlwaysBranchesToDerivedType(mappingData, out derivedTypeMappings))
            {
                return mappingExpressions.Any()
                    ? Expression.Block(mappingExpressions.Concat(derivedTypeMappings))
                    : derivedTypeMappings;
            }

            var mappingExtras = GetMappingExtras(mapperData);

            mappingExpressions.AddUnlessNullOrEmpty(derivedTypeMappings);
            mappingExpressions.AddUnlessNullOrEmpty(mappingExtras.PreMappingCallback);
            mappingExpressions.AddRange(GetObjectPopulation(mappingData));
            mappingExpressions.AddUnlessNullOrEmpty(mappingExtras.PostMappingCallback);

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
            var preMappingCallback = GetMappingCallbackOrNull(Before, basicMapperData, mapperData);
            var postMappingCallback = GetMappingCallbackOrNull(After, basicMapperData, mapperData);
            var mapToNullCondition = GetMapToNullConditionOrNull(mapperData);

            return new MappingExtras(
                mapperData,
                preMappingCallback,
                postMappingCallback,
                mapToNullCondition);
        }

        protected static Expression GetMappingCallbackOrNull(
            CallbackPosition callbackPosition,
            IBasicMapperData basicData,
            IMemberMapperData mapperData)
        {
            return mapperData.MapperContext.UserConfigurations.GetCallbackOrNull(callbackPosition, basicData, mapperData);
        }

        private static Expression GetMapToNullConditionOrNull(IMemberMapperData mapperData)
            => mapperData.MapperContext.UserConfigurations.GetMapToNullConditionOrNull(mapperData);

        protected abstract IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData);

        private Expression GetMappingBlock(IList<Expression> mappingExpressions, MappingExtras mappingExtras)
        {
            var mapperData = mappingExtras.MapperData;

            if (mappingExpressions.None())
            {
                goto CreateFullMappingBlock;
            }

            Expression returnExpression;

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

                var localVariableAssignment = mappingExpressions.First(exp => exp.NodeType == ExpressionType.Assign);

                if (mappingExpressions.Last() == localVariableAssignment)
                {
                    var assignedValue = ((BinaryExpression)localVariableAssignment).Right;
                    returnExpression = GetReturnExpression(assignedValue, mappingExtras);

                    if (mappingExpressions.Count == 1)
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
                var callbackActionType = configuredCallback.Type.GetGenericArguments()[0];

                Type[] contextTypes;
                Expression contextAccess;

                if (callbackActionType.IsGenericType())
                {
                    contextTypes = callbackActionType.GetGenericArguments();
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