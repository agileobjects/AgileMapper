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
                    mapperData.Parameter);
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
                    .Concat(objectPopulation.Actions)
                    .Concat(postMappingCallback)
                    .Concat(returnLabel));

            var wrappedMappingBlock = WrapInTryCatch(mappingBlock, mapperData);

            var finalMappingBlock = EmbedInlineMappersIfAppropriate(wrappedMappingBlock, objectPopulation);

            var mapperLambda = Expression
                .Lambda<MapperFunc<TSource, TTarget>>(finalMappingBlock, data.MapperData.Parameter);

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

        protected abstract ObjectPopulation GetObjectPopulation(IObjectMapperCreationData data);

        protected abstract Expression GetReturnValue(ObjectMapperData data);

        private static Expression WrapInTryCatch(Expression mappingBlock, MemberMapperData data)
        {
            var configuredCallback = data.MapperContext.UserConfigurations.GetExceptionCallbackOrNull(data);
            var exceptionVariable = Parameters.Create<Exception>("ex");

            Expression catchBody;

            if (configuredCallback != null)
            {
                var callbackActionType = configuredCallback.Type.GetGenericArguments()[0];

                Type[] contextTypes;
                Expression contextAccess;

                if (callbackActionType.IsGenericType)
                {
                    contextTypes = callbackActionType.GetGenericArguments();
                    contextAccess = Parameters.GetAppropriateTypedMappingContextAccess(contextTypes, data);
                }
                else
                {
                    contextTypes = new[] { data.SourceType, data.TargetType };
                    contextAccess = data.Parameter;
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
                    data.Parameter,
                    exceptionVariable);

                catchBody = Expression.Throw(mappingExceptionCreation, mappingBlock.Type);
            }

            var catchBlock = Expression.Catch(exceptionVariable, catchBody);

            return Expression.TryCatch(mappingBlock, catchBlock);
        }

        private static Expression EmbedInlineMappersIfAppropriate(Expression mappingBlock, ObjectPopulation objectPopulation)
        {
            if (objectPopulation.InlineObjectMappers.None())
            {
                return mappingBlock;
            }

            var inlineMapperVariables = new List<ParameterExpression>();
            var finalMappingActions = new List<Expression>();

            foreach (var inlineObjectMapper in objectPopulation.InlineObjectMappers)
            {
                var mapperVariableAssignment = Expression.Assign(
                    inlineObjectMapper.MapperVariable,
                    inlineObjectMapper.MapperLambda);

                inlineMapperVariables.Add(inlineObjectMapper.MapperVariable);
                finalMappingActions.Add(mapperVariableAssignment);
            }

            finalMappingActions.Add(mappingBlock);

            var finalMappingBlock = Expression.Block(inlineMapperVariables, finalMappingActions);

            return finalMappingBlock;
        }
    }
}