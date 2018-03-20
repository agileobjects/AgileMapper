namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions.Internal;
    using Members;

    internal abstract class SourceMemberTypeDependentKeyBase
    {
        private Func<IMappingData, bool> _sourceMemberTypeTester;

        public IObjectMappingData MappingData { get; set; }

        public ObjectMapperData MapperData { get; set; }

        public bool HasTypeTester { get; private set; }

        public void AddSourceMemberTypeTesterIfRequired(IObjectMappingData mappingData = null)
        {
            if (mappingData == null)
            {
                mappingData = MappingData;
            }

            if (mappingData.IsPartOfDerivedTypeMapping)
            {
                return;
            }

            var typeTests = mappingData
                .MapperData
                .DataSourcesByTargetMember
                .Values
                .Select(dataSourceSet => dataSourceSet.SourceMemberTypeTest)
                .WhereNotNull()
                .ToArray();

            if (typeTests.None())
            {
                return;
            }

            var typeTest = typeTests.AndTogether();
            var typeTestLambda = Expression.Lambda<Func<IMappingData, bool>>(typeTest, Parameters.MappingData);

            _sourceMemberTypeTester = typeTestLambda.Compile();
            HasTypeTester = true;
        }

        protected bool SourceHasRequiredTypes(IMappingDataOwner otherKey)
            => !HasTypeTester || _sourceMemberTypeTester.Invoke(otherKey.MappingData);
    }
}