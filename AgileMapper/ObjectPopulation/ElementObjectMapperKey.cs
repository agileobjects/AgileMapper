namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal class ElementObjectMapperKey : ObjectMapperKeyBase
    {
        public ElementObjectMapperKey(MappingTypes mappingTypes)
            : base(mappingTypes)
        {
        }

        public override bool Equals(object obj)
        {
            var otherKey = (ElementObjectMapperKey)obj;

            return TypesMatch(otherKey) && SourceHasRequiredTypes(otherKey);
        }

        public override int GetHashCode() => 0;

        protected override ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes)
            => new ElementObjectMapperKey(newMappingTypes);
    }
}