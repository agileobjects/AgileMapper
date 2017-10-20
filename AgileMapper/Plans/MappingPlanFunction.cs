namespace AgileObjects.AgileMapper.Plans
{
    using System.Linq.Expressions;
    using ObjectPopulation;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal static class MappingPlanFunction
    {
        public static string For(Expression lambda, ObjectMapperData mapperData)
        {
            lambda = GetFinalMappingLambda(lambda, mapperData);

            var sourceType = mapperData.SourceType.GetFriendlyName();
            var targetType = mapperData.TargetType.GetFriendlyName();

            return $@"
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// Map {sourceType} -> {targetType}
// Rule Set: {mapperData.RuleSet.Name}
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

{lambda.ToReadableString()}".TrimStart();
        }

        private static Expression GetFinalMappingLambda(Expression lambda, ObjectMapperData mapperData)
        {
            var lambdaWithEnumMismatches = EnumMappingMismatchFinder.Process(lambda, mapperData);

            return lambdaWithEnumMismatches;
        }
    }
}