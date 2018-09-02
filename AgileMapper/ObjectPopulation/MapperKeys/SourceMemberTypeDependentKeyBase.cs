namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    using System;
    using System.Linq;
    using Extensions.Internal;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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
                .Project(dataSourceSet => dataSourceSet.SourceMemberTypeTest)
                .WhereNotNull()
                .ToArray();

            if (typeTests.None())
            {
                return;
            }

            var typeTest = typeTests.AndTogether();
            var mappingDataParameter = typeof(IMappingData).GetOrCreateParameter();
            var typeTestLambda = Expression.Lambda<Func<IMappingData, bool>>(typeTest, mappingDataParameter);

            _sourceMemberTypeTester = typeTestLambda.Compile();
            HasTypeTester = true;
        }

        protected bool SourceHasRequiredTypes(IMappingDataOwner otherKey)
            => !HasTypeTester || _sourceMemberTypeTester.Invoke(otherKey.MappingData);
    }
}