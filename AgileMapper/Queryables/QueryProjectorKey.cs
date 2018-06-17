namespace AgileObjects.AgileMapper.Queryables
{
    using System;
    using Members.Sources;
    using ObjectPopulation;
    using ObjectPopulation.MapperKeys;

    internal class QueryProjectorKey : ObjectMapperKeyBase, IRootMapperKey
    {
        private readonly MapperContext _mapperContext;

        public QueryProjectorKey(
            MappingTypes mappingTypes,
            Type queryProviderType,
            MapperContext mapperContext)
            : base(mappingTypes)
        {
            QueryProviderType = queryProviderType;
            _mapperContext = mapperContext;
        }

        public MappingRuleSet RuleSet => _mapperContext.RuleSets.Project;

        public Type QueryProviderType { get; }

        public override IMembersSource GetMembersSource(ObjectMapperData parentMapperData)
            => _mapperContext.RootMembersSource;

        #region ExcludeFromCodeCoverage
#if DEBUG
        // Create Instance is used to update the Key with runtime 
        // Mapping Types; that never happens with query projection.
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        protected override ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes)
            => new QueryProjectorKey(newMappingTypes, QueryProviderType, _mapperContext);

        public bool Equals(IRootMapperKey otherKey)
        {
            return TypesMatch(otherKey) &&
                (((QueryProjectorKey)otherKey).QueryProviderType == QueryProviderType);
        }
    }
}