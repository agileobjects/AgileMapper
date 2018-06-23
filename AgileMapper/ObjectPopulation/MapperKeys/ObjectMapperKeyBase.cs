namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    using Members.Sources;

    internal abstract class ObjectMapperKeyBase : SourceMemberTypeDependentKeyBase, ITypedMapperKey
    {
        protected ObjectMapperKeyBase(MappingTypes mappingTypes)
        {
            MappingTypes = mappingTypes;
        }

        public MappingTypes MappingTypes { get; }

        protected bool TypesMatch(ITypedMapperKey otherKey) => otherKey.MappingTypes.Equals(MappingTypes);

        public abstract IMembersSource GetMembersSource(ObjectMapperData parentMapperData);

        public ObjectMapperKeyBase WithTypes<TNewSource, TNewTarget>()
        {
            if (MappingTypes.RuntimeTypesAreTheSame)
            {
                return this;
            }

            return CreateInstance(MappingTypes.WithTypes<TNewSource, TNewTarget>());
        }

        protected abstract ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes);

        public bool Equals(ITypedMapperKey otherKey)
            => TypesMatch(otherKey) && SourceHasRequiredTypes(otherKey);
    }
}