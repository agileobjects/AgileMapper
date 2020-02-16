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

        public string GetDescription()
        {
            return $@"
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// Map {_sourceType.GetFriendlyName()} -> {_targetType.GetFriendlyName()}
// Repeated Mapping Mapper
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

{_mapping.ToReadableString()}".TrimStart();
        }
    }
}