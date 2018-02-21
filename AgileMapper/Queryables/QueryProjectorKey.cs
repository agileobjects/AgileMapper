namespace AgileObjects.AgileMapper.Queryables
{
    using System;
    using Members;
    using Members.Sources;
    using ObjectPopulation;

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

        public override IMembersSource GetMembersSource(IObjectMappingData parentMappingData)
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

        public override bool Equals(object obj)
        {
            var otherKey = (IRootMapperKey)obj;

            // ReSharper disable once PossibleNullReferenceException
            return (otherKey.RuleSet == RuleSet) &&
                    TypesMatch(otherKey) &&
                 (((QueryProjectorKey)otherKey).QueryProviderType == QueryProviderType);
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override int GetHashCode() => 0;
    }
}