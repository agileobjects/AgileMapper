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
            IObjectMappingData mappingData,
            out bool isConditional)
        {
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

            return derivedTypeDataSourceSet.BuildValue();
        }
    }
}