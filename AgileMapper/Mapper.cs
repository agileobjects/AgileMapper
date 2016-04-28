namespace AgileObjects.AgileMapper
{
    using Api;

    public sealed class Mapper : IMapper
    {
        private static readonly IMapper _default = new Mapper(MapperContext.Default);

        private readonly MapperContext _mapperContext;

        private Mapper(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        #region Static Access Methods

        public static ResultTypeSelector<TSource> Map<TSource>(TSource source)
        {
            return _default.Map(source);
        }

        #endregion

        ResultTypeSelector<TSource> IMapper.Map<TSource>(TSource source)
        {
            return new ResultTypeSelector<TSource>(source, _mapperContext);
        }
    }
}
