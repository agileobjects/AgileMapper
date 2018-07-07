namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using Extensions.Internal;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class SimpleTypeMappingExpressionFactory : MappingExpressionFactoryBase
    {
        public static readonly MappingExpressionFactoryBase Instance = new SimpleTypeMappingExpressionFactory();

        public override bool IsFor(IObjectMappingData mappingData)
            => mappingData.MappingTypes.TargetType.IsSimple();

        protected override IEnumerable<Expression> GetObjectPopulation(MappingCreationContext context)
        {
            var mapperData = context.MapperData;

            return new[]
            {
                mapperData.CanConvert(mapperData.SourceType, mapperData.TargetType)
                    ? mapperData.GetValueConversion(mapperData.SourceObject, mapperData.TargetType)
                    : mapperData.TargetObject
            };
        }
    }
}