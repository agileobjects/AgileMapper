namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

    internal interface IInlineMapperKey
    {
        MappingTypes MappingTypes { get; }

        MappingRuleSet RuleSet { get; }

        IList<LambdaExpression> Configurations { get; }

        MapperContext CreateInlineMapperContext();
    }
}