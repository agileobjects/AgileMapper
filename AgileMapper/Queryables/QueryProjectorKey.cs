namespace AgileObjects.AgileMapper.Queryables
{
    using System;
    using System.Linq;
    using Members;
    using Members.Sources;
    using ObjectPopulation;

    internal class QueryProjectorKey : ObjectMapperKeyBase
    {
        private readonly Type _queryProviderType;
        private readonly MapperContext _mapperContext;

        public QueryProjectorKey(
            MappingTypes mappingTypes,
            IQueryProvider queryProvider,
            MapperContext mapperContext)
            : this(mappingTypes, queryProvider.GetType(), mapperContext)
        {
        }

        public QueryProjectorKey(
            MappingTypes mappingTypes,
            Type queryProviderType,
            MapperContext mapperContext)
            : base(mappingTypes)
        {
            _queryProviderType = queryProviderType;
            _mapperContext = mapperContext;
        }

        public override IMembersSource GetMembersSource(IObjectMappingData parentMappingData)
            => _mapperContext.RootMembersSource;

        protected override ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes)
            => new QueryProjectorKey(newMappingTypes, _queryProviderType, _mapperContext);

        public override bool Equals(object obj)
        {
            var otherKey = (QueryProjectorKey)obj;

            // ReSharper disable once PossibleNullReferenceException
            return TypesMatch(otherKey) && (otherKey._queryProviderType == _queryProviderType);
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override int GetHashCode() => 0;
    }
}