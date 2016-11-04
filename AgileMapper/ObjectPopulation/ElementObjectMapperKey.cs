namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;
    using Members.Sources;

    internal class ElementObjectMapperKey : ObjectMapperKeyBase
    {
        private IMembersSource _membersSource;

        public ElementObjectMapperKey(MappingTypes mappingTypes, IMembersSource membersSource = null)
            : base(mappingTypes)
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

        public override int GetHashCode() => 0;
    }
}