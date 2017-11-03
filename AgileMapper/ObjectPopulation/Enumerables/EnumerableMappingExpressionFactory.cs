namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;
    using ReadableExpressions;

    internal class EnumerableMappingExpressionFactory : MappingExpressionFactoryBase
    {
        public static readonly MappingExpressionFactoryBase Instance = new EnumerableMappingExpressionFactory();

        public override bool IsFor(IObjectMappingData mappingData)
            => mappingData.MapperKey.MappingTypes.IsEnumerable;

        protected override bool TargetCannotBeMapped(IObjectMappingData mappingData, out Expression nullMappingBlock)
        {
            if (mappingData.MapperData.SourceMember.IsEnumerable)
            {
                return base.TargetCannotBeMapped(mappingData, out nullMappingBlock);
            }

            nullMappingBlock = Expression.Block(
                ReadableExpression.Comment("No source enumerable available"),
                mappingData.MapperData.GetFallbackCollectionValue());

            return true;
        }

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            yield return mappingData.MappingContext.RuleSet.EnumerablePopulationStrategy.GetPopulation(mappingData);
        }

        protected override Expression GetReturnValue(ObjectMapperData mapperData)
            => mapperData.EnumerablePopulationBuilder.GetReturnValue();
    }
}