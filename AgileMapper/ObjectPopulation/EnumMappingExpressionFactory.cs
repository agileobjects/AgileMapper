namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Globalization;
    using Extensions.Internal;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    internal class EnumMappingExpressionFactory : MappingExpressionFactoryBase
    {
        public static readonly EnumMappingExpressionFactory Instance = new EnumMappingExpressionFactory();

        public override bool IsFor(IObjectMappingData mappingData)
            => mappingData.MapperData.TargetType.GetNonNullableType().IsEnum();

        protected override bool TargetCannotBeMapped(IObjectMappingData mappingData, out string reason)
        {
            var mapperData = mappingData.MapperData;

            if (mapperData.CanConvert(mapperData.SourceType, mapperData.TargetType))
            {
                return base.TargetCannotBeMapped(mappingData, out reason);
            }

            reason = string.Format(
                CultureInfo.InvariantCulture,
                "Unable to convert source Type '{0}' to target enum Type '{1}'",
                mapperData.SourceType.GetFriendlyName(),
                mapperData.TargetType.GetFriendlyName());

            return true;
        }

        protected override IEnumerable<Expression> GetObjectPopulation(MappingCreationContext context)
        {
            yield return context.MapperData.GetValueConversion(
                context.MapperData.SourceObject,
                context.MapperData.TargetType);
        }
    }
}