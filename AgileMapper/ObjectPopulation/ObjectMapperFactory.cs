namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Globalization;

    internal class ObjectMapperFactory
    {
        public IObjectMapper<TInstance> CreateFor<TSource, TTarget, TInstance>(IObjectMappingContext omc)
        {
            var mapperKey = string.Format(
                CultureInfo.InvariantCulture,
                "{0} -> {1}, {2}: {3} ObjectMapper",
                typeof(TSource).FullName,
                typeof(TTarget).FullName,
                typeof(TInstance).FullName,
                omc.RuleSetName);

            var mapper = omc.MapperContext.Cache.GetOrAdd(mapperKey, k =>
            {
                var lambda = omc.TargetMember.IsEnumerable
                    ? EnumerableMappingLambdaFactory<TSource, TTarget, TInstance>.Instance.Create(omc)
                    : ComplexTypeMappingLambdaFactory<TSource, TTarget, TInstance>.Instance.Create(omc);

                return new ObjectMapper<TSource, TTarget, TInstance>(lambda);
            });

            return mapper;
        }
    }
}