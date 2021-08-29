namespace AgileObjects.AgileMapper.ObjectPopulation
{
    internal static class StaticMapperCache<TSource, TTarget>
    {
        private static readonly MapperCache _createNew = new MapperCache();
        private static readonly MapperCache _overwrite = new MapperCache();
        private static readonly MapperCache _merge = new MapperCache();

        public static void AddIfAppropriate(ObjectMapper<TSource, TTarget> mapper, IObjectMappingData mappingData)
        {
            if (!mappingData.IsRoot ||
                 mappingData.IsPartOfDerivedTypeMapping ||
                !mappingData.MappingContext.RuleSet.Settings.RootKeysAreStaticallyCacheable ||
                !mapper.IsStaticallyCacheable())
            {
                return;
            }

            var mappingContext = mappingData.MappingContext;
            var ruleSets = mappingContext.MapperContext.RuleSets;

            if (mappingContext.RuleSet == ruleSets.CreateNew)
            {
                _createNew.SetMapperIfAppropriate(mapper);
                return;
            }

            if (mappingContext.RuleSet == ruleSets.Overwrite)
            {
                _overwrite.SetMapperIfAppropriate(mapper);
                return;
            }

            _merge.SetMapperIfAppropriate(mapper);
        }

        public static bool TryGetMapperFor(IMappingContext mappingContext, out ObjectMapper<TSource, TTarget> mapper)
        {
            var mapperContext = mappingContext.MapperContext;
            var ruleSets = mapperContext.RuleSets;

            if (mappingContext.RuleSet == ruleSets.CreateNew)
            {
                return _createNew.TryGetMapper(mapperContext, out mapper);
            }

            if (mappingContext.RuleSet == ruleSets.Overwrite)
            {
                return _overwrite.TryGetMapper(mapperContext, out mapper);
            }

            return _merge.TryGetMapper(mapperContext, out mapper);
        }

        private class MapperCache
        {
            private ObjectMapper<TSource, TTarget> _mapper;

            public void SetMapperIfAppropriate(ObjectMapper<TSource, TTarget> mapper)
            {
                _mapper ??= mapper.WithResetCallback(Reset);
            }

            public bool TryGetMapper(MapperContext mapperContext, out ObjectMapper<TSource, TTarget> mapper)
            {
                if (_mapper?.MapperData.MapperContext != mapperContext)
                {
                    mapper = null;
                    return false;
                }

                mapper = _mapper;
                return true;
            }

            private void Reset() => _mapper = null;
        }
    }
}
