namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;
    using Members.Sources;

    internal abstract class ObjectMapperKeyBase : SourceMemberTypeDependentKeyBase, ITypedMapperKey
    {
        protected ObjectMapperKeyBase(MappingTypes mappingTypes)
        {
            MappingTypes = mappingTypes;
        }

        public MappingTypes MappingTypes { get; }

        protected bool TypesMatch(ITypedMapperKey otherKey) => otherKey.MappingTypes.Equals(MappingTypes);

        public abstract IMembersSource GetMembersSource(IObjectMappingData parentMappingData);

        public ObjectMapperKeyBase WithTypes<TNewSource, TNewTarget>()
        {
            if (MappingTypes.RuntimeTypesAreTheSame)
            {
                return this;
            }

            return CreateInstance(MappingTypes.WithTypes<TNewSource, TNewTarget>());
        }

        protected abstract ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes);
    }
}