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
    using TypeConversion;

    internal class EnumerableMappingExpressionFactory : MappingExpressionFactoryBase
    {
        protected override bool TargetCannotBeMapped(IObjectMappingData mappingData, out string reason)
        {
            var mapperData = mappingData.MapperData;

            if (HasCompatibleSourceMember(mapperData))
            {
                return base.TargetCannotBeMapped(mappingData, out reason);
            }

            if (ConfiguredMappingFactory.HasMappingFactories(mapperData))
            {
                return base.TargetCannotBeMapped(mappingData, out reason);
            }

            if (mappingData.GetToTargetDataSources().Any(ds => ds.SourceMember.IsEnumerable))
            {
                return base.TargetCannotBeMapped(mappingData, out reason);
            }

            reason = "No source enumerable available";
            return true;
        }

        private static bool HasCompatibleSourceMember(IMemberMapperData mapperData)
        {
            if (!mapperData.SourceMember.IsEnumerable)
            {
                return false;
            }

            var sourceElementMember = mapperData.SourceMember.GetElementMember();
            var targetElementMember = mapperData.TargetMember.GetElementMember();

            if (mapperData.CanConvert(sourceElementMember.Type, targetElementMember.Type))
            {
                return true;
            }

            if (!mapperData.MapperContext.UserConfigurations.HasMappingFactories)
            {
                return false;
            }

            var queryContext = new QualifiedMemberContext(
                mapperData.RuleSet,
                sourceElementMember.Type,
                targetElementMember.Type,
                sourceElementMember,
                targetElementMember,
                mapperData,
                mapperData.MapperContext);

            return ConfiguredMappingFactory.HasMappingFactories(queryContext);
        }

        protected override Expression GetNullMappingFallbackValue(IMemberMapperData mapperData)
            => mapperData.GetFallbackCollectionValue();

        protected override IEnumerable<Expression> GetObjectPopulation(MappingCreationContext context)
        {
            if (!HasCompatibleSourceMember(context.MapperData))
            {
                yield break;
            }

            var elementContext = context.MapperData.GetElementMemberContext();

            if (elementContext.IsRepeatMapping() &&
                context.RuleSet.RepeatMappingStrategy.WillNotMap(elementContext))
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