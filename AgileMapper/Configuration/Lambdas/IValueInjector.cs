namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;

    internal interface IValueInjector
    {
        Expression Inject(Type[] contextTypes, IMemberMapperData mapperData);
    }
}