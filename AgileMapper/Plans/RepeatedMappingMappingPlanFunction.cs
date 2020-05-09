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
        private readonly Type _sourceType;
        private readonly Type _targetType;
        private readonly Expression _mapping;

        public RepeatedMappingMappingPlanFunction(IRepeatedMapperFunc mapperFunc)
        {
            _sourceType = mapperFunc.SourceType;
            _targetType = mapperFunc.TargetType;
            _mapping = mapperFunc.Mapping;
        }

        public Expression GetExpression()
        {
            var description = GetMappingDescription();

            return Expression.Block(
                ReadableExpression.Comment(description),
                _mapping);
        }

        public string GetDescription()
        {
            var description = GetMappingDescription(linePrefix: "// ");

            return description + _mapping.ToReadableString();
        }

        private string GetMappingDescription(string linePrefix = null)
        {
            return $@"
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
{linePrefix}Map {_sourceType.GetFriendlyName()} -> {_targetType.GetFriendlyName()}
{linePrefix}Repeated Mapping Mapper
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
{linePrefix}- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

";
        }
    }
}