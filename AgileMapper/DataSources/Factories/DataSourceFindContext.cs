namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using Extensions;
    using Extensions.Internal;
    using Members;

    internal class DataSourceFindContext
    {
        private IList<ConfiguredDataSourceFactory> _relevantConfiguredDataSourceFactories;
        private IList<IConfiguredDataSource> _configuredDataSources;
        private SourceMemberMatchContext _sourceMemberMatchContext;

        public DataSourceFindContext(IChildMemberMappingData memberMappingData)
        {
            MemberMappingData = memberMappingData;
        }

        public MapperContext MapperContext => MemberMapperData.MapperContext;

        public IChildMemberMappingData MemberMappingData { get; private set; }

        public IMemberMapperData MemberMapperData => MemberMappingData.MapperData;

        public QualifiedMember TargetMember => MemberMapperData.TargetMember;

        public int DataSourceIndex { get; set; }

        public bool StopFind { get; set; }

        private IEnumerable<ConfiguredDataSourceFactory> RelevantConfiguredDataSourceFactories
            => _relevantConfiguredDataSourceFactories ??
              (_relevantConfiguredDataSourceFactories = GetRelevantConfiguredDataSourceFactories());

        private IList<ConfiguredDataSourceFactory> GetRelevantConfiguredDataSourceFactories()
        {
            var relevantDataSourceFactories = GetRelevantConfiguredDataSourceFactories(MemberMapperData);

            if (!MemberMapperData.Parent.Context.IsForToTargetMapping)
            {
                return relevantDataSourceFactories;
            }

            var originalChildMapperData = new ChildMemberMapperData(
                TargetMember,
                MemberMapperData.Parent.OriginalMapperData);

            relevantDataSourceFactories = relevantDataSourceFactories.Append(
                GetRelevantConfiguredDataSourceFactories(originalChildMapperData));

            return relevantDataSourceFactories;
        }

        private IList<ConfiguredDataSourceFactory> GetRelevantConfiguredDataSourceFactories(IMemberMapperData mapperData)
            => MapperContext.UserConfigurations.GetRelevantDataSourceFactories(mapperData);

        public IList<IConfiguredDataSource> ConfiguredDataSources
        {
            get
            {
                return _configuredDataSources ?? (_configuredDataSources =
                   RelevantConfiguredDataSourceFactories
                       .FindMatches(MemberMapperData)
                       .Project(MemberMapperData, (md, dsf) => dsf.Create(md))
                       .ToArray());
            }
        }

        public SourceMemberMatch BestSourceMemberMatch { get; set; }

        public IDataSource GetFallbackDataSource()
            => MemberMappingData.RuleSet.FallbackDataSourceFactory.Invoke(MemberMapperData);

        public IDataSource GetFinalDataSource(IDataSource foundDataSource)
            => GetFinalDataSource(foundDataSource, MemberMappingData);

        public IDataSource GetFinalDataSource(IDataSource foundDataSource, IChildMemberMappingData mappingData)
        {
            var childTargetMember = mappingData.MapperData.TargetMember;

            if (UseComplexTypeDataSource(foundDataSource, childTargetMember))
            {
                return ComplexTypeDataSource.Create(foundDataSource, DataSourceIndex, mappingData);
            }

            if (childTargetMember.IsEnumerable && foundDataSource.SourceMember.IsEnumerable)
            {
                return new EnumerableDataSource(foundDataSource, DataSourceIndex, mappingData);
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

        public SourceMemberMatchContext GetSourceMemberMatchContext()
        {
            if (_sourceMemberMatchContext != null)
            {
                return _sourceMemberMatchContext?.With(MemberMappingData);
            }

            return _sourceMemberMatchContext = new SourceMemberMatchContext(MemberMappingData);
        }

        public DataSourceFindContext With(IChildMemberMappingData memberMappingData)
        {
            MemberMappingData = memberMappingData;
            _configuredDataSources = null;
            DataSourceIndex = 0;
            StopFind = false;
            return this;
        }
    }
}