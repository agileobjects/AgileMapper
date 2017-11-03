namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class SimpleTypeMappingExpressionFactory : MappingExpressionFactoryBase
    {
        public static readonly MappingExpressionFactoryBase Instance = new SimpleTypeMappingExpressionFactory();

        public override bool IsFor(IObjectMappingData mappingData)
            => mappingData.MapperKey.MappingTypes.TargetType.IsSimple();

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (mapperData.MapperContext.ValueConverters.CanConvert(mapperData.SourceType, mapperData.TargetType))
            {
                yield return mapperData.GetValueConversion(mapperData.SourceObject, mapperData.TargetType);
                yield break;
            }

            yield return mapperData.TargetObject;
        }
    }
}