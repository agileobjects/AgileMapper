namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Globalization;
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

        protected override void AddObjectPopulation(MappingCreationContext context)
        {
            var mapperData = context.MapperData;
            var enumMapping = mapperData.GetValueConversion(mapperData.SourceObject, mapperData.TargetType);

            var population = context.MapperData.LocalTargetVariable.AssignTo(enumMapping);

            context.MappingExpressions.Add(population);
        }
    }
}