namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

    internal interface IInlineMapperKey
    {
        MappingTypes MappingTypes { get; }

        MappingRuleSet RuleSet { get; }

        Type ConfiguratorType { get; }

        IList<LambdaExpression> Configurations { get; }

        MapperContext CreateInlineMapperContext();
    }
}