namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Reflection;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;

    internal class ExceptionCallback : UserConfiguredItemBase
    {
        private readonly Expression _callback;

        public ExceptionCallback(MappingConfigInfo configInfo, Expression callback)
            : base(configInfo)
        {
            _callback = callback;
        }

        public Expression ToCatchBody(
            Expression exceptionVariable,
            Type returnType,
            IMemberMapperData mapperData)
        {
            var callbackActionType = _callback.Type.GetGenericTypeArguments()[0];

            Type[] contextTypes;
            Expression contextAccess;
            MethodInfo exceptionContextCreateMethod;

            if (callbackActionType.IsGenericType())
            {
                contextTypes = callbackActionType.GetGenericTypeArguments();
                contextAccess = mapperData.GetAppropriateTypedMappingContextAccess(contextTypes);
                exceptionContextCreateMethod = ObjectMappingExceptionData.CreateTypedMethod;
            }
            else
            {
                contextTypes = new[] { mapperData.SourceType, mapperData.TargetType };
                contextAccess = mapperData.MappingDataObject;
                exceptionContextCreateMethod = ObjectMappingExceptionData.CreateMethod;
            }

            var createExceptionContextCall = Expression.Call(
                exceptionContextCreateMethod.MakeGenericMethod(contextTypes),
                contextAccess,
                exceptionVariable);

            var callbackInvocation = Expression.Invoke(_callback, createExceptionContextCall);
            var returnDefault = returnType.ToDefaultExpression();
            var configuredCatchBody = Expression.Block(callbackInvocation, returnDefault);

            return configuredCatchBody;
        }
    }
}