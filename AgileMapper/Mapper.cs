namespace AgileObjects.AgileMapper
{
    using Api;
    using Api.Configuration;

    public sealed class Mapper : IMapper
    {
        private static readonly IMapper _default = Create();

        private readonly MapperContext _mapperContext;

        private Mapper(MapperContext mapperContext)
        {
            _mapperContext = mapperContext;
        }

        #region Factory Methods

        public static IMapper Create()
        {
            return new Mapper(new MapperContext());
        }

        #endregion

        public PostEventConfigStartingPoint After => new PostEventConfigStartingPoint(_mapperContext);

        public MappingConfigStartingPoint WhenMapping => new MappingConfigStartingPoint(_mapperContext);

        #region Static Access Methods

        public static TSource Clone<TSource>(TSource source) where TSource : class
            => _default.Clone(source);

        public static ResultTypeSelector<TSource> Map<TSource>(TSource source) => _default.Map(source);

        #endregion

        TSource IMapper.Clone<TSource>(TSource source) => Map(source).ToNew<TSource>();

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
