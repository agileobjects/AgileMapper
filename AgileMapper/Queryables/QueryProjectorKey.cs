namespace AgileObjects.AgileMapper.Queryables
{
    using System.Linq;
    using Members;
    using Members.Sources;
    using ObjectPopulation;

    internal class QueryProjectorKey : ObjectMapperKeyBase
    {
        private readonly MapperContext _mapperContext;

        public QueryProjectorKey(
            MappingTypes mappingTypes,
            IQueryable sourceQueryable,
            MapperContext mapperContext)
            : base(mappingTypes)
        {
            _mapperContext = mapperContext;
            SourceQueryable = sourceQueryable;
        }

        public IQueryable SourceQueryable { get; }

        public override IMembersSource GetMembersSource(IObjectMappingData parentMappingData)
            => _mapperContext.RootMembersSource;

        protected override ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes)
            => new QueryProjectorKey(newMappingTypes, SourceQueryable, _mapperContext);

        public override bool Equals(object obj)
        {
            var otherKey = (QueryProjectorKey)obj;

            // ReSharper disable once PossibleNullReferenceException
            return TypesMatch(otherKey) &&
                  (otherKey.SourceQueryable.Provider == SourceQueryable.Provider);
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override int GetHashCode() => 0;
    }
}