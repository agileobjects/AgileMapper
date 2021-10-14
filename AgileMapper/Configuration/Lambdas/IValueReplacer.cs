namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;

    internal interface IValueReplacer
    {
        bool HasMappingContextParameter { get; }
        
        Expression Replace(Type[] contextTypes, IMemberMapperData mapperData);
    }
}