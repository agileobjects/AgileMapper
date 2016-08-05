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

    internal abstract class ObjectMappingLambdaFactoryBase<TSource, TTarget>
    {
        public Expression<MapperFunc<TSource, TTarget>> Create(IObjectMappingContext omc)
        {
            var returnLabelTarget = Expression.Label(omc.TargetObject.Type, "Return");
            var returnNull = Expression.Return(returnLabelTarget, Expression.Default(omc.TargetObject.Type));

            if (IsNotConstructable(omc))
            {
                return Expression.Lambda<MapperFunc<TSource, TTarget>>(GetNullMappingBlock(returnNull), omc.Parameter);
            }

            var preMappingCallback = GetMappingCallback(CallbackPosition.Before, omc);
            var shortCircuitReturns = GetShortCircuitReturns(returnNull, omc);
            var objectPopulation = GetObjectPopulation(omc);
            var postMappingCallback = GetMappingCallback(CallbackPosition.After, omc);
            var returnValue = GetReturnValue(omc);
            var returnLabel = Expression.Label(returnLabelTarget, returnValue);

            var mappingBlock = Expression.Block(
                new[] { omc.InstanceVariable },
                preMappingCallback
                    .Concat(shortCircuitReturns)
                    .Concat(objectPopulation)
                    .Concat(postMappingCallback)
                    .Concat(returnLabel));

            var wrappedMappingBlock = WrapInTryCatch(mappingBlock, omc);

            var mapperLambda = Expression
                .Lambda<MapperFunc<TSource, TTarget>>(wrappedMappingBlock, omc.Parameter);

            return mapperLambda;
        }

        private static Expression GetNullMappingBlock(GotoExpression returnNull)
        {
            return Expression.Block(
                ReadableExpression.Comment("Unable to construct object of Type " + returnNull.Value.Type.GetFriendlyName()),
                returnNull.Value);
        }

        protected abstract bool IsNotConstructable(IObjectMappingContext omc);

        private static IEnumerable<Expression> GetMappingCallback(CallbackPosition callbackPosition, IObjectMappingContext omc)
        {
            yield return GetCallbackOrEmpty(c => c.GetCallbackOrNull(callbackPosition, omc), omc);
        }

        protected static Expression GetCallbackOrEmpty(
            Func<UserConfigurationSet, Expression> callbackFactory,
            IMemberMappingContext context)
            => callbackFactory.Invoke(context.MapperContext.UserConfigurations) ?? Constants.EmptyExpression;

        protected abstract IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingContext omc);

        protected abstract IEnumerable<Expression> GetObjectPopulation(IObjectMappingContext omc);

        protected abstract Expression GetReturnValue(IObjectMappingContext omc);

        #region Try / Catch Support

        private static Expression WrapInTryCatch(Expression mappingBlock, IObjectMappingContext omc)
        {
            var configuredCallback = omc.MapperContext.UserConfigurations.GetExceptionCallbackOrNull(omc);
            var exceptionVariable = Parameters.Create<Exception>("ex");

            Expression catchBody;

            if (configuredCallback != null)
            {
                var exceptionContextCreateMethod = MemberMappingExceptionContext
                    .CreateMethod
                    .MakeGenericMethod(omc.SourceType, omc.TargetType);

                var exceptionContextCreateCall = Expression.Call(
                    exceptionContextCreateMethod,
                    omc.Parameter,
                    exceptionVariable);

                var callbackInvocation = Expression.Invoke(configuredCallback, exceptionContextCreateCall);
                var returnDefault = Expression.Default(mappingBlock.Type);
                catchBody = Expression.Block(callbackInvocation, returnDefault);
            }
            else
            {
                var mappingExceptionCreation = Expression.New(
                    MappingException.ConstructorInfo,
                    omc.Parameter,
                    exceptionVariable);

                catchBody = Expression.Throw(mappingExceptionCreation, mappingBlock.Type);
            }

            var catchBlock = Expression.Catch(exceptionVariable, catchBody);

            return Expression.TryCatch(mappingBlock, catchBlock);
        }

        #endregion
    }
}