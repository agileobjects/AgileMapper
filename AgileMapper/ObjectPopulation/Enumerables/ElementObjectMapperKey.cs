namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using Members;
    using Members.Sources;
#if DEBUG
    using ReadableExpressions.Extensions;
#endif

    internal class ElementObjectMapperKey : ObjectMapperKeyBase
    {
        private IMembersSource _membersSource;

        public ElementObjectMapperKey(MappingTypes mappingTypes)
            : base(mappingTypes)
        {
        }

        private ElementObjectMapperKey(MappingTypes mappingTypes, IMembersSource membersSource)
            : this(mappingTypes)
        {
            _membersSource = membersSource;
        }

        public override IMembersSource GetMembersSource(IObjectMappingData parentMappingData)
            => _membersSource ?? (_membersSource = new ElementMembersSource(parentMappingData));

        protected override ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes)
            => new ElementObjectMapperKey(newMappingTypes, _membersSource);

        public override bool Equals(object obj)
        {
            var otherKey = (ElementObjectMapperKey)obj;

            return TypesMatch(otherKey) && SourceHasRequiredTypes(otherKey);
        }

        #region ExcludeFromCodeCoverage
#if CODE_COVERAGE_SUPPORTED
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override int GetHashCode() => 0;

        #region ToString
#if CODE_COVERAGE_SUPPORTED
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var sourceTypeName = MappingTypes.SourceType.GetFriendlyName();
            var targetTypeName = MappingTypes.TargetType.GetFriendlyName();

            return $"[{sourceTypeName}] -> [{targetTypeName}]";
        }
#endif
        #endregion
    }
}