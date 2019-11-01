namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Globalization;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using ReadableExpressions.Extensions;
    using TypeConversion;

    internal class EnumMappingExpressionFactory : MappingExpressionFactoryBase
    {
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
            var mapperData = context.MapperData;
            var enumMapping = mapperData.GetValueConversion(mapperData.SourceObject, mapperData.TargetType);

            yield return context.MapperData.LocalVariable.AssignTo(enumMapping);
        }
    }
}