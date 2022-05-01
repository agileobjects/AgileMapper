namespace AgileObjects.AgileMapper.Configuration;

using System;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Members;

internal abstract class ExceptionCallback : UserConfiguredItemBase
{
    protected ExceptionCallback(MappingConfigInfo configInfo)
        : base(configInfo)
    {
    }

    public abstract Expression ToCatchBody(
        Expression exceptionVariable,
        Type returnType,
        IMemberMapperData mapperData);
}