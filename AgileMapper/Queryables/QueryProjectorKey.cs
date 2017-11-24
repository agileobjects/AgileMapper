namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq;
    using Members;
    using Members.Sources;
    using ObjectPopulation;

    internal class QueryProjectorKey : ObjectMapperKeyBase
    {
        private readonly IQueryProvider _queryProvider;
        private readonly MapperContext _mapperContext;

        public QueryProjectorKey(
            MappingTypes mappingTypes,
            IQueryProvider queryProvider,
            MapperContext mapperContext)
            : base(mappingTypes)
        {
            _queryProvider = queryProvider;
            _mapperContext = mapperContext;
        }

        public override IMembersSource GetMembersSource(IObjectMappingData parentMappingData)
            => _mapperContext.RootMembersSource;

        protected override ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes)
            => new QueryProjectorKey(newMappingTypes, _queryProvider, _mapperContext);

        public override bool Equals(object obj)
        {
            var otherKey = (QueryProjectorKey)obj;

            // ReSharper disable once PossibleNullReferenceException
            return TypesMatch(otherKey) && (otherKey._queryProvider == _queryProvider);
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override int GetHashCode() => 0;
    }
}