namespace AgileObjects.AgileMapper.ObjectPopulation.MapperKeys
{
    using System;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Extensions.Internal;

    internal abstract class SourceMemberTypeDependentKeyBase
    {
        private Func<MappingExecutionContextBase2, bool> _sourceMemberTypeTester;

        public IObjectMappingData MappingData { get; set; }

        public ObjectMapperData MapperData { get; set; }

        public MappingExecutionContextBase2 MappingExecutionContext { get; set; }

        public bool HasTypeTester { get; private set; }

        public void AddSourceMemberTypeTesterIfRequired(IObjectMappingData mappingData = null)
        {
            mappingData ??= MappingData;

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
                .ToList();

            if (typeTests.None())
            {
                return;
            }

            var typeTest = typeTests.AndTogether();
            var contextParameter = MappingExecutionContextConstants.Parameter;
            
            var typeTestLambda = Expression
                .Lambda<Func<MappingExecutionContextBase2, bool>>(typeTest, contextParameter);

            _sourceMemberTypeTester = typeTestLambda.Compile();
            HasTypeTester = true;
        }

        protected bool SourceHasRequiredTypes(IMappingExecutionContextOwner otherKey)
            => !HasTypeTester || _sourceMemberTypeTester.Invoke(otherKey.MappingExecutionContext);
    }
}