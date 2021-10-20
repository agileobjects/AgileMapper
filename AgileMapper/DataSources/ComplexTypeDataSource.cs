namespace AgileObjects.AgileMapper.DataSources
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;
    using Members.Extensions;
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

            return Create(
                mappingData.MapperData.SourceMember,
                mapping,
               (sm, m) => new ComplexTypeDataSource(sm, m));
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

            return Create(
                wrappedDataSource,
                mapping,
               (wds, m) => new ComplexTypeDataSource(wds, m));
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

            return Create(
                sourceMember,
                mapping,
               (sm, m) => new ComplexTypeDataSource(sm, m));
        }

        private static IDataSource Create<TArg>(
            TArg argument,
            Expression mapping,
            Func<TArg, Expression, IDataSource> factory)
        {
            return mapping == Constants.EmptyExpression
                ? NullDataSource.EmptyValue
                : factory.Invoke(argument, mapping);
        }

        #endregion
    }
}