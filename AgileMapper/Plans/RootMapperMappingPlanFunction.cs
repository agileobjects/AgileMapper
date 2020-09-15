namespace AgileObjects.AgileMapper.Plans
{
    using System;
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
        private CommentExpression _summary;
        private Expression _finalMapping;

        public RootMapperMappingPlanFunction(IObjectMapper mapper)
        {
            _mapperData = mapper.MapperData;
            _mapping = mapper.GetMappingLambda();
        }

        public Type SourceType => _mapperData.SourceType;

        public Type TargetType => _mapperData.TargetType;

        public CommentExpression Summary
            => _summary ??= ReadableExpression.Comment(GetMappingDescription());

        private string GetMappingDescription(string linePrefix = null)
        {
            var sourceTypeName = SourceType.GetFriendlyName();
            var targetTypeName = TargetType.GetFriendlyName();

            return $@"
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
{linePrefix}Map {sourceTypeName} -> {targetTypeName}
{linePrefix}Rule Set: {_mapperData.RuleSet.Name}
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

";
        }

        public Expression Mapping
            => _finalMapping ??= GetFinalMappingExpression();

        public string ToSourceCode()
        {
            var description = GetMappingDescription(linePrefix: "// ");

            return description + Mapping.ToReadableString();
        }

        private Expression GetFinalMappingExpression()
        {
            var mappingWithEnumMismatches = EnumMappingMismatchFinder.Process(_mapping, _mapperData);

            return mappingWithEnumMismatches;
        }
    }
}