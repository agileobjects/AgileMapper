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

            var mappingExpressions = GetShortCircuitReturns(returnNull, mapperData).ToList();

            Expression derivedTypeMappings;

            if (MappingAlwaysBranchesToDerivedType(mappingData, out derivedTypeMappings))
            {
                return mappingExpressions.Any()
                    ? Expression.Block(mappingExpressions.Concat(derivedTypeMappings))
                    : derivedTypeMappings;
            }

            var basicMapperData = mapperData.WithNoTargetMember();

            mappingExpressions.AddUnlessNullOrEmpty(derivedTypeMappings);
            mappingExpressions.AddUnlessNullOrEmpty(GetMappingCallbackOrNull(CallbackPosition.Before, basicMapperData, mapperData));
            mappingExpressions.AddRange(GetObjectPopulation(mappingData));
            mappingExpressions.AddUnlessNullOrEmpty(GetMappingCallbackOrNull(CallbackPosition.After, basicMapperData, mapperData));

            var mappingBlock = GetMappingBlock(mappingExpressions, mapperData);
            var mappingBlockWithTryCatch = WrapInTryCatch(mappingBlock, mapperData);

            return mappingBlockWithTryCatch;
        }

        protected abstract bool TargetCannotBeMapped(IObjectMappingData mappingData, out Expression nullMappingBlock);

        protected abstract IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, ObjectMapperData mapperData);

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

        protected static Expression GetMappingCallbackOrNull(
            CallbackPosition callbackPosition,
            IBasicMapperData basicData,
            IMemberMapperData mapperData)
        {
            return mapperData.MapperContext.UserConfigurations.GetCallbackOrNull(callbackPosition, basicData, mapperData);
        }

        protected abstract IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData);

        private Expression GetMappingBlock(IList<Expression> mappingExpressions, ObjectMapperData mapperData)
        {
            if (mappingExpressions[0].NodeType != ExpressionType.Block)
            {
                var objectAssignment = mappingExpressions.First(exp => exp.NodeType == ExpressionType.Assign);

                if (mappingExpressions.Last() == objectAssignment)
                {
                    var assignedValue = ((BinaryExpression)objectAssignment).Right;

                    if (mappingExpressions.Count == 1)
                    {
                        return Expression.Block(assignedValue);
                    }

                    mappingExpressions[mappingExpressions.Count - 1] = assignedValue;

                    return Expression.Block(mappingExpressions);
                }
            }

            mappingExpressions.Add(Expression.Label(mapperData.ReturnLabelTarget, GetReturnValue(mapperData)));

            var mappingBlock = Expression.Block(new[] { mapperData.InstanceVariable }, mappingExpressions);

            return mappingBlock;
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
    }
}