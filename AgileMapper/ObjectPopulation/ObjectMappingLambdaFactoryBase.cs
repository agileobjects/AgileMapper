namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Configuration;
    using Extensions;
    using Members;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal abstract class ObjectMappingLambdaFactoryBase
    {
        public Expression<MapperFunc<TSource, TTarget>> Create<TSource, TTarget>(IObjectMapperCreationData data)
        {
            var mapperData = data.MapperData;

            var returnLabelTarget = Expression.Label(mapperData.TargetObject.Type, "Return");
            var returnNull = Expression.Return(returnLabelTarget, Expression.Default(mapperData.TargetObject.Type));

            if (IsNotConstructable(data))
            {
                return Expression.Lambda<MapperFunc<TSource, TTarget>>(
                    GetNullMappingBlock(returnNull),
                    mapperData.MdParameter);
            }

            var basicMappingData = BasicMapperData.WithNoTargetMember(mapperData);

            var preMappingCallback = GetMappingCallback(CallbackPosition.Before, basicMappingData, mapperData);
            var shortCircuitReturns = GetShortCircuitReturns(returnNull, mapperData);
            var objectPopulation = GetObjectPopulation(data);
            var postMappingCallback = GetMappingCallback(CallbackPosition.After, basicMappingData, mapperData);
            var returnValue = GetReturnValue(mapperData);
            var returnLabel = Expression.Label(returnLabelTarget, returnValue);

            var mappingBlock = Expression.Block(
                new[] { mapperData.InstanceVariable },
                preMappingCallback
                    .Concat(shortCircuitReturns)
                    .Concat(objectPopulation)
                    .Concat(postMappingCallback)
                    .Concat(returnLabel));

            var wrappedMappingBlock = WrapInTryCatch(mappingBlock, mapperData);

            var mapperLambda = Expression
                .Lambda<MapperFunc<TSource, TTarget>>(wrappedMappingBlock, data.MapperData.MdParameter);

            return mapperLambda;
        }

        private static Expression GetNullMappingBlock(GotoExpression returnNull)
        {
            return Expression.Block(
                ReadableExpression.Comment("Unable to construct object of Type " + returnNull.Value.Type.GetFriendlyName()),
                returnNull.Value);
        }

        protected abstract bool IsNotConstructable(IObjectMapperCreationData data);

        private static IEnumerable<Expression> GetMappingCallback(
            CallbackPosition callbackPosition,
            BasicMapperData basicData,
            MemberMapperData data)
        {
            yield return GetCallbackOrEmpty(c => c.GetCallbackOrNull(callbackPosition, basicData, data), data);
        }

        protected static Expression GetCallbackOrEmpty(
            Func<UserConfigurationSet, Expression> callbackFactory,
            MemberMapperData data)
            => callbackFactory.Invoke(data.MapperContext.UserConfigurations) ?? Constants.EmptyExpression;

        protected abstract IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, ObjectMapperData data);

        protected abstract IEnumerable<Expression> GetObjectPopulation(IObjectMapperCreationData data);

        protected abstract Expression GetReturnValue(ObjectMapperData data);

        #region Try / Catch Support

        private static Expression WrapInTryCatch(Expression mappingBlock, MemberMapperData data)
        {
            var configuredCallback = data.MapperContext.UserConfigurations.GetExceptionCallbackOrNull(data);
            var exceptionVariable = Parameters.Create<Exception>("ex");

            Expression catchBody;

            if (configuredCallback != null)
            {
                var exceptionContextCreateMethod = MappingExceptionContextData
                    .CreateMethod
                    .MakeGenericMethod(data.SourceType, data.TargetType);

                var exceptionContextCreateCall = Expression.Call(
                    exceptionContextCreateMethod,
                    data.MdParameter,
                    exceptionVariable);

                var callbackInvocation = Expression.Invoke(configuredCallback, exceptionContextCreateCall);
                var returnDefault = Expression.Default(mappingBlock.Type);
                catchBody = Expression.Block(callbackInvocation, returnDefault);
            }
            else
            {
                var mappingExceptionCreation = Expression.New(
                    MappingException.ConstructorInfo,
                    data.MdParameter,
                    exceptionVariable);

                catchBody = Expression.Throw(mappingExceptionCreation, mappingBlock.Type);
            }

            var catchBlock = Expression.Catch(exceptionVariable, catchBody);

            return Expression.TryCatch(mappingBlock, catchBlock);
        }

        #endregion
    }
}