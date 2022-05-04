namespace AgileObjects.AgileMapper.Members.Extensions;

using System;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using AgileMapper.Extensions.Internal;

internal static class MemberExpressionExtensions
{
    public static bool IsMappingContextCall(this MethodCallExpression methodCall)
        => methodCall.Object == Constants.ExecutionContextParameter;

    public static TryExpression WrapInTryCatch(this Expression mapping, IMemberMapperData mapperData)
    {
        var configuredCallback = mapperData
            .MapperContext.UserConfigurations
            .GetExceptionCallbackOrNull(mapperData);

        var exceptionVariable = typeof(Exception)
            .GetOrCreateParameter(mapperData.GetExceptionName());

        if (configuredCallback == null)
        {
            var catchBody = Expression.Throw(
                MappingException.GetFactoryMethodCall(mapperData, exceptionVariable),
                mapping.Type);

            return CreateTryCatch(mapping, exceptionVariable, catchBody);
        }

        var configuredCatchBody = configuredCallback
            .ToCatchBody(exceptionVariable, mapping.Type, mapperData);

        return CreateTryCatch(
            mapping,
            exceptionVariable,
            configuredCatchBody);
    }

    private static string GetExceptionName(this IMemberMapperData mapperData)
    {
        var suffix = mapperData.TargetMember.Depth;

        if (mapperData.Context.IsForDerivedType)
        {
            ++suffix;
        }

        if (suffix == 1)
        {
            return "ex";
        }

        return "ex" + suffix;
    }

    private static TryExpression CreateTryCatch(
        Expression mappingBlock,
        ParameterExpression exceptionVariable,
        Expression catchBody)
    {
        var catchBlock = Expression.Catch(exceptionVariable, catchBody);

        return Expression.TryCatch(mappingBlock, catchBlock);
    }
}