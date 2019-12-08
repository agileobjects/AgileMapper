namespace AgileObjects.AgileMapper.Queryables.Recursion
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;
    using ObjectPopulation;
    using ObjectPopulation.RepeatedMappings;

    internal struct MapToDepthRepeatMappingStrategy : IRepeatMappingStrategy
    {
        public bool AppliesTo(IQualifiedMemberContext context)
            => !context.TargetMemberIsEnumerableElement();

        public bool WillNotMap(IQualifiedMemberContext context)
            => AppliesTo(context) && ShortCircuitRecursion(context);

        public Expression GetMapRepeatedCallFor(
            IObjectMappingData childMappingData,
            MappingValues mappingValues,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData)
        {
            if (ShortCircuitRecursion(childMappingData.MapperData))
            {
                return GetMappingShortCircuit(childMappingData);
            }

            var inlineMappingBlock = MappingFactory.GetInlineMappingBlock(
                childMappingData,
                mappingValues,
                MappingDataCreationFactory.ForChild(mappingValues, 0, childMappingData.MapperData));

            return inlineMappingBlock;
        }

        private static bool ShortCircuitRecursion(IQualifiedMemberContext context)
        {
            if (!context.TargetMember.IsRecursion)
            {
                return false;
            }

            return context
                .MapperContext
                .UserConfigurations
                .ShortCircuitRecursion(context);
        }

        private static Expression GetMappingShortCircuit(IObjectMappingData childMappingData)
        {
            if (childMappingData.MapperData.TargetMember.IsComplex)
            {
                return Constants.EmptyExpression;
            }

            var helper = childMappingData.MapperData.EnumerablePopulationBuilder.TargetTypeHelper;

            return helper.GetEmptyInstanceCreation();
        }
    }
}