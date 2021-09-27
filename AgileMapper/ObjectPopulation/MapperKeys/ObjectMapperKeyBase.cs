namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    using System;
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

        public IObjectMappingData CreateMappingData()
        {
            var mappingData = MappingExecutionContext.ToMappingData();
            mappingData.MapperKey = this;
            return mappingData;
        }

        public ObjectMapperKeyBase WithTypes(Type newSourceType, Type newTargetType, bool forceNewKey)
        {
            if (!forceNewKey && MappingTypes.RuntimeTypesAreTheSame)
            {
                return this;
            }

            return CreateInstance(MappingTypes.WithTypes(newSourceType, newTargetType));
        }

        protected abstract ObjectMapperKeyBase CreateInstance(MappingTypes newMappingTypes);

        public bool Equals(ITypedMapperKey otherKey)
            => TypesMatch(otherKey) && SourceHasRequiredTypes(otherKey);
    }
}