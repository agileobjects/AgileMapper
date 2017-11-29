namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Linq.Expressions;
    using ObjectPopulation.Recursion;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal class RecursionMapperMappingPlanFunction : IMappingPlanFunction
    {
        private readonly Type _sourceType;
        private readonly Type _targetType;
        private readonly Expression _mappingLambda;

        public RecursionMapperMappingPlanFunction(IRecursionMapperFunc mapperFunc)
        {
            _sourceType = mapperFunc.SourceType;
            _targetType = mapperFunc.TargetType;
            _mappingLambda = mapperFunc.MappingLambda;
        }

        public string GetDescription()
        {
            return $@"
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// Map {_sourceType.GetFriendlyName()} -> {_targetType.GetFriendlyName()}
// Recursion Mapper
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 

{_mappingLambda.ToReadableString(_ => _.NoQuotedLambdaComments)}".TrimStart();
        }
    }
}