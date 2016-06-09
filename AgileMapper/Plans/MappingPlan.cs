namespace AgileObjects.AgileMapper.Plans
{
    using ObjectPopulation;
    using ReadableExpressions;

    internal class MappingPlan
    {
        public static string For<TSource, TTarget>(MappingContext mappingContext)
        {
            var rootOmc = ObjectMappingContextFactory
                .CreateRoot(default(TSource), default(TTarget), mappingContext);

            var rootMapper = mappingContext.MapperContext.ObjectMapperFactory
                .CreateFor<TSource, TTarget, TTarget>(rootOmc);

            return rootMapper.MappingLambda.ToReadableString();
        }
    }
}
