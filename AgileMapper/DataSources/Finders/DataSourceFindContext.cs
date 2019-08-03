namespace AgileObjects.AgileMapper.DataSources.Finders
{
    using System.Collections.Generic;
    using Extensions;
    using Extensions.Internal;
    using Members;

    internal class DataSourceFindContext
    {
        public DataSourceFindContext(IChildMemberMappingData childMappingData)
        {
            ChildMappingData = childMappingData;

            ConfiguredDataSources = GetConfiguredDataSources(MapperData);

            if (!MapperData.Parent.Context.IsForToTargetMapping)
            {
                return;
            }

            var originalChildMapperData = new ChildMemberMapperData(
                MapperData.TargetMember,
                MapperData.Parent.OriginalMapperData);

            ConfiguredDataSources = ConfiguredDataSources.Append(
                GetConfiguredDataSources(originalChildMapperData));
        }

        private IList<IConfiguredDataSource> GetConfiguredDataSources(IMemberMapperData mapperData)
            => MapperContext.UserConfigurations.GetDataSources(mapperData);

        public IChildMemberMappingData ChildMappingData { get; }

        public IMemberMapperData MapperData => ChildMappingData.MapperData;

        public MapperContext MapperContext => MapperData.MapperContext;

        public int DataSourceIndex { get; set; }

        public bool StopFind { get; set; }

        public IList<IConfiguredDataSource> ConfiguredDataSources { get; }

        public IDataSource GetFallbackDataSource()
            => ChildMappingData.RuleSet.FallbackDataSourceFactory.Create(MapperData);

        public IDataSource GetFinalDataSource(IDataSource foundDataSource)
            => GetFinalDataSource(foundDataSource, ChildMappingData);

        public IDataSource GetFinalDataSource(IDataSource foundDataSource, IChildMemberMappingData mappingData)
        {
            var childTargetMember = mappingData.MapperData.TargetMember;

            if (UseComplexTypeDataSource(foundDataSource, childTargetMember))
            {
                return new ComplexTypeMappingDataSource(foundDataSource, DataSourceIndex, mappingData);
            }

            if (childTargetMember.IsEnumerable && foundDataSource.SourceMember.IsEnumerable)
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

            if ((dataSource.Value.Type == targetMember.Type) &&
                (dataSource.SourceMember.Type != targetMember.Type))
            {
                return false;
            }

            return !targetMember.Type.IsFromBcl();
        }
    }
}