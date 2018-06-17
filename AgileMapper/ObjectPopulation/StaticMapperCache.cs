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

            var ruleSets = mappingData.MappingContext.MapperContext.RuleSets;

            if (mappingData.MappingContext.RuleSet == ruleSets.CreateNew)
            {
                _createNew.SetMapperIfAppropriate(mapper);
                return;
            }

            if (mappingData.MappingContext.RuleSet == ruleSets.Overwrite)
            {
                _overwrite.SetMapperIfAppropriate(mapper);
                return;
            }

            _merge.SetMapperIfAppropriate(mapper);
        }

        public static bool TryGetMapperFor(IObjectMappingData mappingData, out ObjectMapper<TSource, TTarget> mapper)
        {
            var ruleSets = mappingData.MappingContext.MapperContext.RuleSets;

            if (mappingData.MappingContext.RuleSet == ruleSets.CreateNew)
            {
                return _createNew.TryGetMapper(mappingData, out mapper);
            }

            if (mappingData.MappingContext.RuleSet == ruleSets.Overwrite)
            {
                return _overwrite.TryGetMapper(mappingData, out mapper);
            }

            return _merge.TryGetMapper(mappingData, out mapper);
        }

        private class MapperCache
        {
            private ObjectMapper<TSource, TTarget> _mapper;

            public void SetMapperIfAppropriate(ObjectMapper<TSource, TTarget> mapper)
            {
                if (_mapper == null)
                {
                    _mapper = mapper.WithResetCallback(Reset);
                }
            }

            public bool TryGetMapper(IObjectMappingData key, out ObjectMapper<TSource, TTarget> mapper)
            {
                if (_mapper?.MapperData.MapperContext != key.MappingContext.MapperContext)
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
