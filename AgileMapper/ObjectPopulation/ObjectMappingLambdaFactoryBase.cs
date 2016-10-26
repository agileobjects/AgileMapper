namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Configuration;
    using Members;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal abstract class ObjectMappingLambdaFactoryBase
    {
        public Expression<MapperFunc<TSource, TTarget>> Create<TSource, TTarget>(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            var returnNull = Expression.Return(
                mapperData.ReturnLabelTarget,
                Expression.Default(mapperData.TargetType));

            if (TargetTypeIsNotConstructable(mappingData))
            {
                return Expression.Lambda<MapperFunc<TSource, TTarget>>(
                    GetNullMappingBlock(returnNull),
                    mapperData.MappingDataObject);
            }

            var mappingExpressions = new List<Expression>();
            var basicMapperData = BasicMapperData.WithNoTargetMember(mapperData);

            mappingExpressions.AddRange(GetShortCircuitReturns(returnNull, mapperData));
            mappingExpressions.Add(GetTypeTests(mapperData));
            mappingExpressions.Add(GetMappingCallback(CallbackPosition.Before, basicMapperData, mapperData));
            mappingExpressions.AddRange(GetObjectPopulation(mappingData));
            mappingExpressions.Add(GetMappingCallback(CallbackPosition.After, basicMapperData, mapperData));
            mappingExpressions.Add(Expression.Label(mapperData.ReturnLabelTarget, GetReturnValue(mapperData)));

            var mappingBlock = Expression.Block(new[] { mapperData.InstanceVariable }, mappingExpressions);
            var mappingBlockWithTryCatch = WrapInTryCatch(mappingBlock, mapperData);

            var mapperLambda = Expression
                .Lambda<MapperFunc<TSource, TTarget>>(mappingBlockWithTryCatch, mapperData.MappingDataObject);

            return mapperLambda;
        }

        private static Expression GetNullMappingBlock(GotoExpression returnNull)
        {
            return Expression.Block(
                ReadableExpression.Comment("Unable to construct object of Type " + returnNull.Value.Type.GetFriendlyName()),
                returnNull.Value);
        }

        protected abstract bool TargetTypeIsNotConstructable(IObjectMappingData mappingData);

        protected abstract IEnumerable<Expression> GetShortCircuitReturns(
            GotoExpression returnNull,
            ObjectMapperData mapperData);

        protected abstract Expression GetTypeTests(ObjectMapperData mapperData);

        private static Expression GetMappingCallback(
            CallbackPosition callbackPosition,
            IBasicMapperData basicData,
            IMemberMapperData mapperData)
        {
            return GetCallbackOrEmpty(c => c.GetCallbackOrNull(callbackPosition, basicData, mapperData), mapperData);
        }

        protected static Expression GetCallbackOrEmpty(
            Func<UserConfigurationSet, Expression> callbackFactory,
            IMemberMapperData mapperData)
            => callbackFactory.Invoke(mapperData.MapperContext.UserConfigurations) ?? Constants.EmptyExpression;

        protected abstract IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData);

        protected abstract Expression GetReturnValue(ObjectMapperData mapperData);

        private static Expression WrapInTryCatch(Expression mappingBlock, ObjectMapperData mapperData)
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
                var mappingExceptionCreation = Expression.New(
                    MappingException.ConstructorInfo,
                    mapperData.MappingDataObject,
                    exceptionVariable);

                catchBody = Expression.Throw(mappingExceptionCreation, mappingBlock.Type);
            }

            var catchBlock = Expression.Catch(exceptionVariable, catchBody);

            return Expression.TryCatch(mappingBlock, catchBlock);
        }
    }
}