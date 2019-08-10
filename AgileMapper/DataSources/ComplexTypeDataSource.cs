namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
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
        private ComplexTypeDataSource(
            IDataSource wrappedDataSource, 
            Expression mapping,
            IList<IDataSource> childDataSources)
            : base(wrappedDataSource, mapping)
        {
            ChildDataSources = childDataSources;
        }

        private ComplexTypeDataSource(
            IQualifiedMember sourceMember,
            Expression mapping,
            IList<IDataSource> childDataSources)
            : base(sourceMember, mapping)
        {
            ChildDataSources = childDataSources;
        }

        #region Factory Methods

        public static IDataSource Create(IObjectMappingData mappingData)
        {
            var mapping = ComplexTypeMappingExpressionFactory.Instance.Create(mappingData);

            return new ComplexTypeDataSource(
                mappingData.MapperData.SourceMember,
                mapping,
                DerivedComplexTypeDataSourcesFactory.CreateFor(mappingData));
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

            return new ComplexTypeDataSource(
                wrappedDataSource,
                mapping,
                Enumerable<IDataSource>.EmptyArray);
        }

        public static IDataSource Create(int dataSourceIndex, IChildMemberMappingData complexTypeMappingData)
        {
            var complexTypeMapperData = complexTypeMappingData.MapperData;
            var relativeMember = complexTypeMapperData.SourceMember.RelativeTo(complexTypeMapperData.SourceMember);
            var sourceMemberAccess = relativeMember.GetQualifiedAccess(complexTypeMapperData);

            var mapping = MappingFactory.GetChildMapping(
                relativeMember,
                sourceMemberAccess,
                dataSourceIndex,
                complexTypeMappingData);

            return new ComplexTypeDataSource(
                complexTypeMappingData.MapperData.SourceMember,
                mapping,
                Enumerable<IDataSource>.EmptyArray);
        }

        #endregion

        public override IList<IDataSource> ChildDataSources { get; }
    }

    internal static class DerivedComplexTypeDataSourcesFactory
    {
        public static IList<IDataSource> CreateFor(IObjectMappingData declaredTypeMappingData)
        {
            return Enumerable<IDataSource>.EmptyArray;
        }
    }
}