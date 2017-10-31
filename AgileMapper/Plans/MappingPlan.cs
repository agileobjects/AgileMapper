namespace AgileObjects.AgileMapper.Plans
{
    using System.Linq.Expressions;
    using ObjectPopulation;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    /// <summary>
    /// Contains details of the mapping plan for a mapping between a particular source and target type,
    /// for a particular mapping type (create new, merge, overwrite).
    /// </summary>
    public class MappingPlan : IMappingPlan
    {
        private readonly IObjectMapper _mapper;

        internal MappingPlan(IObjectMapper cachedMapper)
        {
            _mapper = cachedMapper;
        }

        internal static MappingPlan For<TSource, TTarget>(IMappingContext mappingContext)
        {
            var mappingData = ObjectMappingDataFactory
                .ForRootFixedTypes(default(TSource), default(TTarget), mappingContext);

            return new MappingPlan(mappingData.Mapper);
        }

        /// <summary>
        /// Converts the given <paramref name="mappingPlan"/> to its string representation.
        /// </summary>
        /// <param name="mappingPlan">The <see cref="MappingPlan"/> to convert.</param>
        /// <returns>The string representation of the given <paramref name="mappingPlan"/>.</returns>
        public static implicit operator string(MappingPlan mappingPlan)
        {
            var mapperData = mappingPlan._mapper.MapperData;

            var lambda = GetFinalMappingLambda(mappingPlan._mapper.MappingLambda, mapperData);

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

        /// <summary>
        /// Returns the string representation of the <see cref="MappingPlan"/>.
        /// </summary>
        /// <returns>The string representation of the <see cref="MappingPlan"/>.</returns>
        public override string ToString() => this;
    }
}
