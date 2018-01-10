namespace AgileObjects.AgileMapper.Configuration.Inline
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal interface IInlineConfigurationSet
    {
        int Count { get; }

        IList<LambdaExpression> Lambdas { get; }

        void Apply(IMappingContext mappingContext);

        void Apply(MappingRuleSet ruleSet, MapperContext mapperContext);
    }
}