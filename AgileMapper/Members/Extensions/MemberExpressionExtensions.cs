namespace AgileObjects.AgileMapper.Members.Extensions;

using System;
using System.Linq;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using AgileMapper.Extensions.Internal;
#if NET35
using static Microsoft.Scripting.Ast.Expression;
#else
using static System.Linq.Expressions.Expression;
#endif

internal static class MemberExpressionExtensions
{
    public static bool IsMappingContextCall(this MethodCallExpression methodCall)
        => methodCall.Object == Constants.ExecutionContextParameter;

    public static Expression WrapInTryCatch(this Expression mapping, IMemberMapperData mapperData)
    {
        var configuredCallback = mapperData
            .MapperContext.UserConfigurations
            .GetExceptionCallbackOrNull(mapperData);

        var exceptionVariable = typeof(Exception)
            .GetOrCreateParameter(mapperData.GetExceptionName());

        if (configuredCallback == null)
        {
            var catchBody = Throw(
                MappingException.GetFactoryMethodCall(mapperData, exceptionVariable),
                mapping.Type);

            return CreateTryCatch(mapping, exceptionVariable, catchBody);
        }

        var configuredCatchBody = configuredCallback
            .ToCatchBody(exceptionVariable, mapping.Type, mapperData);

        if (mapping.NodeType != ExpressionType.Block ||
            !mapperData.Context.UseLocalTargetVariable)
        {
            return CreateTryCatch(mapping, exceptionVariable, configuredCatchBody);
        }

        var targetVariable = (ParameterExpression)mapperData.TargetInstance;
        var mappingBlock = (BlockExpression)mapping;

        mapping = Block(
            mappingBlock.Variables.Except(new[] { targetVariable }),
            mappingBlock.Expressions);

        return Block(
            new[] { targetVariable },
            CreateTryCatch(mapping, exceptionVariable, configuredCatchBody));
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
        var catchBlock = Catch(exceptionVariable, catchBody);

        return TryCatch(mappingBlock, catchBlock);
    }
}