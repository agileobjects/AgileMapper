namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ConfiguredFactoryMappingExpressionFactory : MappingExpressionFactoryBase
    {
        protected override IEnumerable<Expression> GetObjectPopulation(MappingCreationContext context)
        {
            var mappingFactories = context
                .MapperContext
                .UserConfigurations
                .QueryMappingFactories(context.MapperData);

            foreach (var mappingFactory in mappingFactories)
            {
                yield return mappingFactory.Create(context.MapperData);
            }
        }
    }
}
