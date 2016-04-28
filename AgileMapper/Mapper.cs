namespace AgileObjects.AgileMapper
{
    using Api;
    using Api.Configuration;

    public sealed class Mapper : IMapper
    {
        private static readonly IMapper _default = new Mapper();

        private readonly MapperContext _mapperContext;

        public Mapper()
            : this(new MapperContext())
        {
        }

        private Mapper(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        public ConfigStartingPoint When => new ConfigStartingPoint(_mapperContext);

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

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
