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

        public Expression GetExpression()
        {
            var description = GetMappingDescription();
            var mapping = GetFinalMappingExpression();

            return Expression.Block(
                ReadableExpression.Comment(description),
                mapping);
        }

        public string GetDescription()
        {
            var description = GetMappingDescription(linePrefix: "// ");
            var mapping = GetFinalMappingExpression();

            return description + mapping.ToReadableString();
        }

        private string GetMappingDescription(string linePrefix = null)
        {
            var sourceType = _mapperData.SourceType.GetFriendlyName();
            var targetType = _mapperData.TargetType.GetFriendlyName();

            return $@"
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
{linePrefix}Map {sourceType} -> {targetType}
{linePrefix}Rule Set: {_mapperData.RuleSet.Name}
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

";
        }

        private Expression GetFinalMappingExpression()
        {
            var mappingWithEnumMismatches = EnumMappingMismatchFinder.Process(_mapping, _mapperData);

            return mappingWithEnumMismatches;
        }
    }
}