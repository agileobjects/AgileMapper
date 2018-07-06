namespace AgileObjects.AgileMapper.Plans
{
    using ObjectPopulation;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;
    using Validation;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class RootMapperMappingPlanFunction : IMappingPlanFunction
    {
        private readonly ObjectMapperData _mapperData;
        private readonly Expression _mappingLambda;

        public RootMapperMappingPlanFunction(IObjectMapper mapper)
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