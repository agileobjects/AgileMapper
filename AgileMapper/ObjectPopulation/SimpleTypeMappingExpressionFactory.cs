namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class SimpleTypeMappingExpressionFactory : MappingExpressionFactoryBase
    {
        public override bool IsFor(IObjectMappingData mappingData)
        {
            return mappingData.MapperKey.MappingTypes.TargetType.IsSimple();
        }

        protected override bool TargetCannotBeMapped(IObjectMappingData mappingData, out Expression nullMappingBlock)
        {
            nullMappingBlock = null;
            return false;
        }

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingData mappingData)
            => Enumerable<Expression>.Empty;

        protected override Expression GetDerivedTypeMappings(IObjectMappingData mappingData)
            => Constants.EmptyExpression;

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

        protected override Expression GetReturnValue(ObjectMapperData mapperData) => mapperData.TargetInstance;
    }
}