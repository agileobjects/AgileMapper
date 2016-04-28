namespace AgileObjects.AgileMapper.Api
{
    public class ResultTypeSelector<TSource>
    {
        private readonly TSource _source;
        private readonly MapperContext _mapperContext;

        internal ResultTypeSelector(TSource source, MapperContext mapperContext)
        {
            _source = source;
            _mapperContext = mapperContext;
        }

        public TResult ToNew<TResult>()
        {
            var mappingContext = new MappingContext(_mapperContext.RuleSets.CreateNew, _mapperContext);

            return mappingContext.MapStart(_source, default(TResult));
        }
    }
}