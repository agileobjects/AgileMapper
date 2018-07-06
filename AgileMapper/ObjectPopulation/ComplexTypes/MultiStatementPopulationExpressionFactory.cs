namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System.Collections.Generic;
    using Members;
    using Members.Population;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using static CallbackPosition;

    internal class MultiStatementPopulationExpressionFactory : PopulationExpressionFactoryBase
    {
        protected override IEnumerable<Expression> GetPopulationExpressionsFor(
            IMemberPopulator memberPopulator,
            IObjectMappingData mappingData)
        {
            var prePopulationCallback = GetPopulationCallbackOrNull(Before, memberPopulator, mappingData);

            if (prePopulationCallback != null)
            {
                yield return prePopulationCallback;
            }

            yield return memberPopulator.GetPopulation();

            var postPopulationCallback = GetPopulationCallbackOrNull(After, memberPopulator, mappingData);

            if (postPopulationCallback != null)
            {
                yield return postPopulationCallback;
            }
        }

        private static Expression GetPopulationCallbackOrNull(
            CallbackPosition position,
            IMemberPopulator memberPopulator,
            IObjectMappingData mappingData)
        {
            return memberPopulator.MapperData.GetMappingCallbackOrNull(position, mappingData.MapperData);
        }
    }
}