﻿namespace AgileObjects.AgileMapper.Queryables.Recursion
{
    using Members;
    using ObjectPopulation;
    using ObjectPopulation.RepeatedMappings;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal struct MapToDepthRepeatMappingStrategy : IRepeatMappingStrategy
    {
        public bool AppliesTo(IBasicMapperData mapperData)
            => !mapperData.TargetMemberIsEnumerableElement();

        public bool WillNotMap(IBasicMapperData mapperData)
            => AppliesTo(mapperData) && ShortCircuitRecursion(mapperData);

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

        private static bool ShortCircuitRecursion(IBasicMapperData mapperData)
        {
            if (!mapperData.TargetMember.IsRecursion)
            {
                return false;
            }

            return ((IMemberMapperData)mapperData.Parent)
                .MapperContext
                .UserConfigurations
                .ShortCircuitRecursion(mapperData);
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