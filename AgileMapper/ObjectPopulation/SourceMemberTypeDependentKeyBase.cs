namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal abstract class SourceMemberTypeDependentKeyBase
    {
        private Func<IMappingData, bool> _sourceMemberTypeTester;

        public IObjectMappingData MappingData { get; set; }

        public void AddSourceMemberTypeTesterIfRequired(IObjectMappingData mappingData = null)
        {
            var typeTests = (mappingData ?? MappingData)
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
        }

        protected bool SourceHasRequiredTypes(SourceMemberTypeDependentKeyBase otherKey)
            => (_sourceMemberTypeTester == null) || _sourceMemberTypeTester.Invoke(otherKey.MappingData);
    }
}