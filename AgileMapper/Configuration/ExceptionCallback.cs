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
        private readonly bool _isGlobalCallback;
        private readonly Type[] _contextTypes;

        public ExceptionCallback(MappingConfigInfo configInfo, Expression callback)
            : base(configInfo)
        {
            _callback = callback;

            var callbackActionType = _callback.Type.GetGenericTypeArguments()[0];
            _isGlobalCallback = !callbackActionType.IsGenericType();

            if (_isGlobalCallback)
            {
                return;
            }

            _contextTypes = callbackActionType.GetGenericTypeArguments();
        }

        public Expression ToCatchBody(
            Expression exceptionVariable,
            Type returnType,
            IMemberMapperData mapperData)
        {
            Type[] contextTypes;
            Expression mappingData;
            MethodInfo exceptionContextCreateMethod;

            if (_isGlobalCallback)
            {
                contextTypes = new[] { mapperData.SourceType, mapperData.TargetType };
                mappingData = mapperData.GetToMappingDataCall(contextTypes);
                exceptionContextCreateMethod = ObjectMappingExceptionData.CreateMethod;
            }
            else
            {
                contextTypes = _contextTypes;
                mappingData = mapperData.GetAppropriateTypedMappingContextAccess(contextTypes);
                exceptionContextCreateMethod = ObjectMappingExceptionData.CreateTypedMethod;
            }

            var createExceptionContextCall = Expression.Call(
                exceptionContextCreateMethod.MakeGenericMethod(contextTypes),
                mappingData,
                exceptionVariable);

            var callbackInvocation = Expression.Invoke(_callback, createExceptionContextCall);
            var returnDefault = returnType.ToDefaultExpression();
            var configuredCatchBody = Expression.Block(callbackInvocation, returnDefault);

            return configuredCatchBody;
        }
    }
}