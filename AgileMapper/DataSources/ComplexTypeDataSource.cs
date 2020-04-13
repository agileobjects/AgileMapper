namespace AgileObjects.AgileMapper.DataSources
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;
    using ObjectPopulation;
    using ObjectPopulation.ComplexTypes;

    internal class ComplexTypeDataSource : DataSourceBase
    {
        private ComplexTypeDataSource(IDataSource wrappedDataSource, Expression mapping)
            : base(wrappedDataSource, mapping)
        {
        }

        private ComplexTypeDataSource(IQualifiedMember sourceMember, Expression mapping)
            : base(sourceMember, mapping)
        {
        }

        #region Factory Methods

        public static IDataSource Create(IObjectMappingData mappingData)
        {
            var mapping = ComplexTypeMappingExpressionFactory.Instance.Create(mappingData);

            return new ComplexTypeDataSource(mappingData.MapperData.SourceMember, mapping);
        }

        public static IDataSource Create(
            IDataSource wrappedDataSource,
            int dataSourceIndex,
            IChildMemberMappingData complexTypeMappingData)
        {
            var mapping = MappingFactory.GetChildMapping(
                wrappedDataSource.SourceMember,
                wrappedDataSource.Value,
                dataSourceIndex,
                complexTypeMappingData);

            return new ComplexTypeDataSource(wrappedDataSource, mapping);
        }

        public static IDataSource Create(int dataSourceIndex, IChildMemberMappingData complexTypeMappingData)
        {
            var complexTypeMapperData = complexTypeMappingData.MapperData;
            var sourceMember = complexTypeMapperData.SourceMember;

            var sourceMemberAccess = sourceMember.GetRelativeQualifiedAccess(
                complexTypeMapperData,
                out var relativeMember);

            var mapping = MappingFactory.GetChildMapping(
                relativeMember,
                sourceMemberAccess,
                dataSourceIndex,
                complexTypeMappingData);

            return new ComplexTypeDataSource(sourceMember, mapping);
        }

        #endregion
    }
}