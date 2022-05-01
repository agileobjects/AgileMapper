namespace AgileObjects.AgileMapper.Configuration;

using System;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Extensions.Internal;
using Members;

internal class ExceptionSwallower : ExceptionCallback
{
    public ExceptionSwallower(MappingConfigInfo configInfo) :
        base(configInfo)
    {
    }

    public override Expression ToCatchBody(
        Expression exceptionVariable,
        Type returnType,
        IMemberMapperData mapperData)
    {
        return returnType.ToDefaultExpression();
    }
}