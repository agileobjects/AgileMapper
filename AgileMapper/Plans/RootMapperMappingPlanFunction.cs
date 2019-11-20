namespace AgileObjects.AgileMapper.Plans
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using ObjectPopulation;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;
    using Validation;

    internal class RootMapperMappingPlanFunction : IMappingPlanFunction
    {
        private readonly ObjectMapperData _mapperData;
        private readonly Expression _mapping;

        public RootMapperMappingPlanFunction(IObjectMapper mapper)
        {
            _mapperData = mapper.MapperData;
            _mapping = mapper.GetMappingLambda();
        }

        public string GetDescription()
        {
            var mapping = GetFinalMappingExpression();

            var sourceType = _mapperData.SourceType.GetFriendlyName();
            var targetType = _mapperData.TargetType.GetFriendlyName();

            return $@"
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// Map {sourceType} -> {targetType}
// Rule Set: {_mapperData.RuleSet.Name}
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

{mapping.ToReadableString()}".TrimStart();
        }

        private Expression GetFinalMappingExpression()
        {
            var mappingWithEnumMismatches = EnumMappingMismatchFinder.Process(_mapping, _mapperData);

            return mappingWithEnumMismatches;
        }
    }
}