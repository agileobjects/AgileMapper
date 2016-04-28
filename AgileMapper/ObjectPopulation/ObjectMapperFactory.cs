namespace AgileObjects.AgileMapper.ObjectPopulation
{
    internal class ObjectMapperFactory
    {
        public IObjectMapper<TTarget> CreateFor<TSource, TTarget>(IObjectMappingContext omc)
        {
            var lambda = omc.TargetMember.IsEnumerable
                ? EnumerableMappingLambdaFactory<TSource, TTarget>.Instance.Create(omc)
                : ComplexTypeMappingLambdaFactory<TSource, TTarget>.Instance.Create(omc);

            var mapper = new ObjectMapper<TSource, TTarget>(lambda);

            return mapper;
        }
    }
}