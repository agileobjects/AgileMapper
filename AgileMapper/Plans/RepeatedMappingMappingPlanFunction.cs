namespace AgileObjects.AgileMapper.Plans
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using ObjectPopulation.RepeatedMappings;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal class RepeatedMappingMappingPlanFunction : IMappingPlanFunction
    {
        private CommentExpression _summary;

        public RepeatedMappingMappingPlanFunction(IRepeatedMapperFunc mapperFunc)
        {
            SourceType = mapperFunc.SourceType;
            TargetType = mapperFunc.TargetType;
            Mapping = mapperFunc.Mapping;
        }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public CommentExpression Summary
            => _summary ??= ReadableExpression.Comment(GetMappingDescription());

        private string GetMappingDescription(string linePrefix = null)
        {
            return $@"
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
{linePrefix}Map {SourceType.GetFriendlyName()} -> {TargetType.GetFriendlyName()}
{linePrefix}Repeated Mapping Mapper
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

";
        }

        public Expression Mapping { get; }

        public string ToSourceCode()
        {
            var description = GetMappingDescription(linePrefix: "// ");

            return description + Mapping.ToReadableString();
        }
    }
}