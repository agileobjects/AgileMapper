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
        private IMapperKeyData _keyData;
        private IObjectMappingData _mappingData;
        private ObjectMapperData _mapperData;
        private Func<object, bool> _sourceMemberTypeTester;

        public IObjectMappingData MappingData
        {
            get => _mappingData ??= KeyData.GetMappingData();
            set => _mappingData = value;
        }

        public ObjectMapperData MapperData
        {
            get => _mapperData ??= MappingData.MapperData;
            set => _mapperData = value;
        }

        public IMapperKeyData KeyData
        {
            get => _keyData ??= _mappingData;
            set => _keyData = value;
        }

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
            var sourceParameter = typeof(object).GetOrCreateSourceParameter();
            var typeTestLambda = Expression.Lambda<Func<object, bool>>(typeTest, sourceParameter);

            _sourceMemberTypeTester = typeTestLambda.Compile();
            HasTypeTester = true;
        }

        protected bool SourceHasRequiredTypes(IMapperKeyDataOwner otherKey)
            => !HasTypeTester || _sourceMemberTypeTester.Invoke(otherKey.KeyData.Source);
    }
}