namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;
    using ReadableExpressions;

    internal class EnumerableMappingExpressionFactory : MappingExpressionFactoryBase
    {
        protected override bool TargetCannotBeMapped(IObjectMappingData mappingData, out Expression nullMappingBlock)
        {
            if (mappingData.MapperData.SourceMember.IsEnumerable)
            {
                nullMappingBlock = null;
                return false;
            }

            nullMappingBlock = Expression.Block(
                ReadableExpression.Comment("No source enumerable available"),
                mappingData.MapperData.GetFallbackCollectionValue());

            return true;
        }

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, ObjectMapperData mapperData)
            => Enumerable.Empty<Expression>();

        protected override Expression GetDerivedTypeMappings(IObjectMappingData mappingData)
            => Constants.EmptyExpression;

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            yield return mappingData.MappingContext.RuleSet.EnumerablePopulationStrategy.GetPopulation(mappingData);
        }

        protected override Expression GetReturnValue(ObjectMapperData mapperData)
            => mapperData.EnumerablePopulationBuilder.GetReturnValue();
    }
}