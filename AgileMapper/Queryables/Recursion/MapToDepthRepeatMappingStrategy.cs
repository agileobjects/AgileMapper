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
        public bool AppliesTo(IMemberMapperData mapperData)
            => !mapperData.TargetMemberIsEnumerableElement();

        public Expression GetMapRepeatedCallFor(
            IObjectMappingData childMappingData,
            MappingValues mappingValues,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData)
        {
            if (ShortCircuitRecursion(childMappingData))
            {
                return GetMappingShortCircuit(childMappingData);
            }

            var inlineMappingBlock = MappingFactory.GetInlineMappingBlock(
                childMappingData,
                mappingValues,
                MappingDataCreationFactory.ForChild(mappingValues, 0, childMappingData.MapperData));

            return inlineMappingBlock;
        }

        private static bool ShortCircuitRecursion(IObjectMappingData childMappingData)
        {
            if (!childMappingData.MapperData.TargetMember.IsRecursion)
            {
                return false;
            }

            return childMappingData
                .MappingContext
                .MapperContext
                .UserConfigurations
                .ShortCircuitRecursion(childMappingData.MapperData);
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