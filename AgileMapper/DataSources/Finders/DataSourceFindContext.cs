namespace AgileObjects.AgileMapper.DataSources.Finders
{
    using System.Collections.Generic;
    using Extensions.Internal;
    using Members;

    internal class DataSourceFindContext
    {
        public DataSourceFindContext(IChildMemberMappingData childMappingData)
        {
            ChildMappingData = childMappingData;

            ConfiguredDataSources = MapperData
                .MapperContext
                .UserConfigurations
                .GetDataSources(MapperData);
        }

        public IChildMemberMappingData ChildMappingData { get; }

        public IMemberMapperData MapperData => ChildMappingData.MapperData;

        public int DataSourceIndex { get; set; }

        public bool StopFind { get; set; }

        public IList<IConfiguredDataSource> ConfiguredDataSources { get; }

        public IDataSource GetFallbackDataSource()
            => ChildMappingData.RuleSet.FallbackDataSourceFactory.Create(MapperData);

        public IDataSource GetFinalDataSource(IDataSource foundDataSource, IChildMemberMappingData mappingData = null)
        {
            if (mappingData == null)
            {
                mappingData = ChildMappingData;
            }

            var childTargetMember = mappingData.MapperData.TargetMember;

            if (UseComplexTypeDataSource(foundDataSource, childTargetMember))
            {
                return new ComplexTypeMappingDataSource(foundDataSource, DataSourceIndex, mappingData);
            }

            if (childTargetMember.IsEnumerable)
            {
                return new EnumerableMappingDataSource(foundDataSource, DataSourceIndex, mappingData);
            }

            return foundDataSource;
        }

        private static bool UseComplexTypeDataSource(IDataSource dataSource, QualifiedMember targetMember)
        {
            if (!targetMember.IsComplex)
            {
                return false;
            }

            if (targetMember.IsDictionary)
            {
                return true;
            }

            if (targetMember.Type == typeof(object))
            {
                return !dataSource.SourceMember.Type.IsSimple();
            }

            return !targetMember.Type.IsFromBcl();
        }
    }
}