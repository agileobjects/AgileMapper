namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;
    using Members.Population;
    using static CallbackPosition;

    internal class MultiStatementPopulationExpressionFactory : PopulationExpressionFactoryBase
    {
        protected override IEnumerable<Expression> GetPopulationExpressionsFor(
            IMemberPopulation memberPopulation,
            IObjectMappingData mappingData)
        {
            var prePopulationCallback = GetPopulationCallbackOrNull(Before, memberPopulation, mappingData);

            if (prePopulationCallback != null)
            {
                yield return prePopulationCallback;
            }

            yield return memberPopulation.GetPopulation();

            var postPopulationCallback = GetPopulationCallbackOrNull(After, memberPopulation, mappingData);

            if (postPopulationCallback != null)
            {
                yield return postPopulationCallback;
            }
        }

        private static Expression GetPopulationCallbackOrNull(
            CallbackPosition position,
            IMemberPopulation memberPopulation,
            IObjectMappingData mappingData)
        {
            return memberPopulation.MapperData.GetMappingCallbackOrNull(position, mappingData.MapperData);
        }
    }
}