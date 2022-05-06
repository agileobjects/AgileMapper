namespace AgileObjects.AgileMapper.Configuration;

using System;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using System.Reflection;
using Extensions.Internal;
using Members;
using NetStandardPolyfills;
using ObjectPopulation;

internal class ConfiguredExceptionCallback : ExceptionCallback
{
    private readonly Expression _callback;
    private readonly bool _isGlobal;
    private readonly Type[] _contextTypes;

    public ConfiguredExceptionCallback(
        MappingConfigInfo configInfo,
        Expression callback) :
        base(configInfo)
    {
        _callback = callback;

        var callbackActionType = _callback.Type.GetGenericTypeArguments()[0];
        _isGlobal = !callbackActionType.IsGenericType();

        if (_isGlobal)
        {
            return;
        }

        _contextTypes = callbackActionType.GetGenericTypeArguments();
    }

    public override Expression ToCatchBody(
        Expression exceptionVariable,
        Type returnType,
        IMemberMapperData mapperData)
    {
        Type[] contextTypes;
        MethodInfo exceptionContextCreateMethod;

        if (_isGlobal)
        {
            contextTypes = new[] { mapperData.SourceType, mapperData.TargetType };
            exceptionContextCreateMethod = ObjectMappingExceptionData.CreateMethod;
        }
        else
        {
            contextTypes = _contextTypes;
            mapperData = mapperData.GetMapperDataFor(contextTypes);
            exceptionContextCreateMethod = ObjectMappingExceptionData.CreateTypedMethod;
        }

        var mappingData = mapperData.GetToMappingDataCall(contextTypes);

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