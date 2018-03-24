namespace AgileObjects.AgileMapper.Plans
{
    using System.Linq.Expressions;
    using ObjectPopulation;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;
    using Validation;

    internal class RootMappingPlanFunction : IMappingPlanFunction
    {
        private readonly ObjectMapperData _mapperData;
        private readonly Expression _mappingLambda;

        public RootMappingPlanFunction(IObjectMapper mapper)
        {
            _mapperData = mapper.MapperData;
            _mappingLambda = mapper.MappingLambda;
        }

        public string GetDescription()
        {
            var lambda = GetFinalMappingLambda();

            var sourceType = _mapperData.SourceType.GetFriendlyName();
            var targetType = _mapperData.TargetType.GetFriendlyName();

            return $@"
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// Map {sourceType} -> {targetType}
// Rule Set: {_mapperData.RuleSet.Name}
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

{lambda.ToReadableString()}".TrimStart();
        }

        private Expression GetFinalMappingLambda()
        {
            var lambdaWithEnumMismatches = EnumMappingMismatchFinder.Process(_mappingLambda, _mapperData);

            return lambdaWithEnumMismatches;
        }
    }
}