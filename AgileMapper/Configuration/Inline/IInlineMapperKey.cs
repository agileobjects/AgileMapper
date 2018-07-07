namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal interface IInlineMapperKey
    {
        MappingTypes MappingTypes { get; }

        MappingRuleSet RuleSet { get; }

        Type ConfiguratorType { get; }

        IList<LambdaExpression> Configurations { get; }

        MapperContext CreateInlineMapperContext();
    }
}