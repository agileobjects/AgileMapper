namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes.ShortCircuits
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using DataSources.Factories;
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal static class DerivedComplexTypeMappingFactory
    {
        public static Expression GetMappingOrNull(
            MappingCreationContext context,
            out bool isConditional)
        {
            var mappingData = context.MappingData;
            var derivedTypeDataSources = DerivedComplexTypeDataSourcesFactory.CreateFor(mappingData);

            if (derivedTypeDataSources.None())
            {
                isConditional = false;
                return null;
            }

            var derivedTypeDataSourceSet = DataSourceSet.For(
                derivedTypeDataSources,
                mappingData,
                ValueExpressionBuilders.ValueSequence);

            isConditional =
                derivedTypeDataSources.Last().IsConditional &&
               !mappingData.MapperData.TargetType.IsAbstract();

            if (!isConditional)
            {
                context.MappingComplete = true;
            }

            return derivedTypeDataSourceSet.BuildValue();
        }
    }
}