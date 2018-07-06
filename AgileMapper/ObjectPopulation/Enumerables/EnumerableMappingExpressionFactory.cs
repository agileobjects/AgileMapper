namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Collections.Generic;
    using Extensions.Internal;
    using Members;
    using ReadableExpressions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class EnumerableMappingExpressionFactory : MappingExpressionFactoryBase
    {
        public static readonly MappingExpressionFactoryBase Instance = new EnumerableMappingExpressionFactory();

        public override bool IsFor(IObjectMappingData mappingData)
            => mappingData.MapperData.TargetMember.IsEnumerable;

        protected override bool TargetCannotBeMapped(IObjectMappingData mappingData, out Expression nullMappingBlock)
        {
            if (mappingData.MapperData.SourceMember.IsEnumerable)
            {
                return base.TargetCannotBeMapped(mappingData, out nullMappingBlock);
            }

            if (HasConfiguredRootDataSources(mappingData.MapperData, out var configuredRootDataSources) &&
                configuredRootDataSources.Any(ds => ds.SourceMember.IsEnumerable))
            {
                return base.TargetCannotBeMapped(mappingData, out nullMappingBlock);
            }

            nullMappingBlock = Expression.Block(
                ReadableExpression.Comment("No source enumerable available"),
                mappingData.MapperData.GetFallbackCollectionValue());

            return true;
        }

        protected override IEnumerable<Expression> GetObjectPopulation(MappingCreationContext context)
        {
            if (!context.MapperData.SourceMember.IsEnumerable)
            {
                yield break;
            }

            yield return context.RuleSet.EnumerablePopulationStrategy.GetPopulation(
                context.MapperData.EnumerablePopulationBuilder,
                context.MappingData);
        }

        protected override Expression GetReturnValue(ObjectMapperData mapperData)
            => mapperData.EnumerablePopulationBuilder.GetReturnValue();
    }
}