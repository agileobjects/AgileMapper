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
    using ReadableExpressions;

    internal abstract class MappingExpressionFactoryBase
    {
        public Expression Create(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (TargetCannotBeMapped(mappingData))
            {
                return Expression.Block(
                    ReadableExpression.Comment(GetNullMappingComment(mapperData.TargetType)),
                    GetNullMappingReturnValue(mapperData));
            }

            var returnNull = Expression.Return(
                mapperData.ReturnLabelTarget,
                Expression.Default(mapperData.TargetType));

            var mappingExpressions = new List<Expression>();
            var basicMapperData = mapperData.WithNoTargetMember();

            mappingExpressions.AddRange(GetShortCircuitReturns(returnNull, mapperData));
            mappingExpressions.AddUnlessNullOrEmpty(GetTypeTests(mappingData));
            mappingExpressions.AddUnlessNullOrEmpty(GetMappingCallbackOrNull(CallbackPosition.Before, basicMapperData, mapperData));
            mappingExpressions.AddRange(GetObjectPopulation(mappingData).WhereNotNull());
            mappingExpressions.AddUnlessNullOrEmpty(GetMappingCallbackOrNull(CallbackPosition.After, basicMapperData, mapperData));

            var mappingBlock = GetMappingBlock(mappingExpressions.WhereNotNull().ToList(), mapperData);
            var mappingBlockWithTryCatch = WrapInTryCatch(mappingBlock, mapperData);

            return mappingBlockWithTryCatch;
        }

        protected abstract bool TargetCannotBeMapped(IObjectMappingData mappingData);

        protected abstract string GetNullMappingComment(Type targetType);

        protected abstract Expression GetNullMappingReturnValue(ObjectMapperData mapperData);

        protected abstract IEnumerable<Expression> GetShortCircuitReturns(
            GotoExpression returnNull,
            ObjectMapperData mapperData);

        protected abstract Expression GetTypeTests(IObjectMappingData mappingData);

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
                    var assignment = (BinaryExpression)objectAssignment;
                    var assignedValue = assignment.Right;

                    if (assignedValue.NodeType == ExpressionType.Invoke)
                    {
                        assignedValue = assignedValue.Replace(mapperData.InstanceVariable, mapperData.TargetObject);
                    }

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
                var returnDefault = Expression.Default(mappingBlock.Type);
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
    }
}