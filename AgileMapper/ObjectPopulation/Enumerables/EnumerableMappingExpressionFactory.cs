namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;

    internal class EnumerableMappingExpressionFactory : MappingExpressionFactoryBase
    {
        protected override bool TargetCannotBeMapped(IObjectMappingData mappingData, out string reason)
        {
            var mapperData = mappingData.MapperData;

            if (HasCompatibleSourceMember(mapperData))
            {
                return base.TargetCannotBeMapped(mappingData, out reason);
            }

            if (HasConfiguredToTargetDataSources(mapperData, out var configuredRootDataSources) &&
                configuredRootDataSources.Any(ds => ds.SourceMember.IsEnumerable))
            {
                return base.TargetCannotBeMapped(mappingData, out reason);
            }

            reason = "No source enumerable available";
            return true;
        }

        private static bool HasCompatibleSourceMember(IMemberMapperData mapperData)
        {
            return mapperData.SourceMember.IsEnumerable &&
                   mapperData.CanConvert(
                       mapperData.SourceMember.GetElementMember().Type,
                       mapperData.TargetMember.GetElementMember().Type);
        }

        protected override Expression GetNullMappingFallbackValue(IMemberMapperData mapperData)
            => mapperData.GetFallbackCollectionValue();

        protected override IEnumerable<Expression> GetObjectPopulation(MappingCreationContext context)
        {
            if (!HasCompatibleSourceMember(context.MapperData))
            {
                yield break;
            }

            var elementMapperData = context.MapperData.GetElementMapperData();

            if (elementMapperData.IsRepeatMapping() &&
                context.RuleSet.RepeatMappingStrategy.WillNotMap(elementMapperData))
            {
                yield break;
            }

            yield return context.RuleSet.EnumerablePopulationStrategy.Invoke(
                context.MapperData.EnumerablePopulationBuilder,
                context.MappingData);
        }

        protected override Expression GetReturnValue(ObjectMapperData mapperData)
            => mapperData.EnumerablePopulationBuilder.GetReturnValue();
    }
}