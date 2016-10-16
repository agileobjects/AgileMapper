namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
                Expression.Default(mappingData.TargetType));

            if (IsNotConstructable(mappingData))
            {
                return Expression.Lambda<MapperFunc<TSource, TTarget>>(
                    GetNullMappingBlock(returnNull),
                    mapperData.Parameter);
            }

            var mappingExpressions = new List<Expression>();
            var basicMapperData = BasicMapperData.WithNoTargetMember(mapperData);

            mappingExpressions.Add(GetMappingCallback(CallbackPosition.Before, basicMapperData, mapperData));
            mappingExpressions.AddRange(GetShortCircuitReturns(returnNull, mapperData));
            mappingExpressions.AddRange(GetObjectPopulation(mappingData));
            mappingExpressions.Add(GetMappingCallback(CallbackPosition.After, basicMapperData, mapperData));
            mappingExpressions.Add(Expression.Label(mapperData.ReturnLabelTarget, GetReturnValue(mapperData)));

            var mappingBlock = Expression.Block(new[] { mapperData.InstanceVariable }, mappingExpressions);

            var wrappedMappingBlock = WrapInTryCatch(mappingBlock, mapperData);

            var mapperLambda = Expression
                .Lambda<MapperFunc<TSource, TTarget>>(wrappedMappingBlock, mapperData.Parameter);

            return mapperLambda;
        }

        private static Expression GetNullMappingBlock(GotoExpression returnNull)
        {
            return Expression.Block(
                ReadableExpression.Comment("Unable to construct object of Type " + returnNull.Value.Type.GetFriendlyName()),
                returnNull.Value);
        }

        protected abstract bool IsNotConstructable(IObjectMappingData mappingData);

        private static Expression GetMappingCallback(
            CallbackPosition callbackPosition,
            IBasicMapperData basicData,
            MemberMapperData mapperData)
        {
            return GetCallbackOrEmpty(c => c.GetCallbackOrNull(callbackPosition, basicData, mapperData), mapperData);
        }

        protected static Expression GetCallbackOrEmpty(
            Func<UserConfigurationSet, Expression> callbackFactory,
            MemberMapperData mapperData)
            => callbackFactory.Invoke(mapperData.MapperContext.UserConfigurations) ?? Constants.EmptyExpression;

        protected abstract IEnumerable<Expression> GetShortCircuitReturns(
            GotoExpression returnNull,
            ObjectMapperData mapperData);

        protected abstract IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData);

        protected abstract Expression GetReturnValue(ObjectMapperData mapperData);

        private static Expression WrapInTryCatch(Expression mappingBlock, MemberMapperData mapperData)
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
                    contextAccess = mapperData.Parameter;
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
                    mapperData.Parameter,
                    exceptionVariable);

                catchBody = Expression.Throw(mappingExceptionCreation, mappingBlock.Type);
            }

            var catchBlock = Expression.Catch(exceptionVariable, catchBody);

            return Expression.TryCatch(mappingBlock, catchBlock);
        }
    }
}