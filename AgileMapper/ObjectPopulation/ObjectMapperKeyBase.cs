namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Members;

    internal abstract class ObjectMapperKeyBase
    {
        private Func<IMappingData, bool> _sourceMemberTypeTester;

        protected ObjectMapperKeyBase(MappingTypes mappingTypes)
        {
            MappingTypes = mappingTypes;
        }

        public MappingTypes MappingTypes { get; }

        public IObjectMappingData MappingData { get; set; }

        protected bool TypesMatch(ObjectMapperKeyBase otherKey) => otherKey.MappingTypes.Equals(MappingTypes);

        public void AddSourceMemberTypeTester(Func<IMappingData, bool> tester)
            => _sourceMemberTypeTester = tester;

        public bool SourceHasRequiredTypes(ObjectMapperKeyBase otherKey)
            => (_sourceMemberTypeTester == null) || _sourceMemberTypeTester.Invoke(otherKey.MappingData);

        public IObjectMapper CreateMapper<TSource, TTarget>()
        {
            var mapper = MappingData.MappingContext.MapperContext
                .ObjectMapperFactory
                .Create<TSource, TTarget>(MappingData);

            MappingData = mapper.MapperData.MappingData = null;

            return mapper;
        }
    }
}