namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;

    internal static class ConfiguredSimpleTypeFactoryDataSourcesFactory
    {
        public static IEnumerable<IDataSource> Create(DataSourceFindContext context)
        {
            var bestMatchingSourceMember = context.BestSourceMemberMatch.SourceMember;
            var mapperData = context.MemberMapperData;

            if (!context.TargetMember.IsSimple ||
                (bestMatchingSourceMember == null) ||
                !mapperData.MapperContext.UserConfigurations.HasSimpleTypeObjectFactories)
            {
                yield break;
            }

            var matcherMapperData = new BasicMapperData(
                mapperData.RuleSet,
                bestMatchingSourceMember.Type,
                context.TargetMember.Type,
                bestMatchingSourceMember,
                context.TargetMember,
                mapperData.Parent);

            var matchingObjectFactories = mapperData
                .MapperContext
                .UserConfigurations
                .QueryObjectFactories(matcherMapperData);

            var objectMapperData = default(IMemberMapperData);

            foreach (var objectFactory in matchingObjectFactories)
            {
                if (objectMapperData == null)
                {
                    var objectMappingData = ObjectMappingDataFactory.ForChild(
                        bestMatchingSourceMember,
                        context.TargetMember,
                        context.DataSourceIndex,
                        context.MemberMappingData.Parent);

                    objectMapperData = objectMappingData.MapperData;
                }

                var sourceMemberValue = bestMatchingSourceMember.GetQualifiedAccess(mapperData);

                var replacementsByTarget = new ExpressionReplacementDictionary(3)
                {
                    [objectMapperData.SourceObject] = sourceMemberValue,
                    [objectMapperData.TargetObject] = mapperData.GetTargetMemberAccess(),
                    [objectMapperData.EnumerableIndex] = mapperData.EnumerableIndex
                };

                var objectCreation = objectFactory
                    .Create(objectMapperData)
                    .Replace(replacementsByTarget);
                
                var condition = objectFactory
                    .GetConditionOrNull(objectMapperData)?
                    .Replace(replacementsByTarget);

                if (bestMatchingSourceMember.Type.CanBeNull())
                {
                    var sourceIsNonNull = sourceMemberValue.GetIsNotDefaultComparison();

                    condition = (condition != null)
                        ? Expression.AndAlso(sourceIsNonNull, condition)
                        : sourceIsNonNull;
                }

                yield return new AdHocDataSource(
                    bestMatchingSourceMember,
                    objectCreation,
                    condition);

                if (condition == null)
                {
                    yield break;
                }
            }
        }
    }
}