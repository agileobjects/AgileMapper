namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System.Collections.Generic;
    using System.Linq;
    using Configuration;
    using Configuration.DataSources;
    using Extensions;
    using Extensions.Internal;
    using Members;

    internal class DataSourceFindContext : IDataSourceSetInfo
    {
        private IList<ConfiguredDataSourceFactory> _relevantConfiguredDataSourceFactories;
        private IList<IConfiguredDataSource> _configuredDataSources;
        private SourceMemberMatchContext _sourceMemberMatchContext;
        private SourceMemberMatch _bestSourceMemberMatch;
        private IDataSource _matchingSourceMemberDataSource;
        private bool? _useSourceMemberDataSource;

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
            => _relevantConfiguredDataSourceFactories ??= GetRelevantConfiguredDataSourceFactories();

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
                return _configuredDataSources ??= RelevantConfiguredDataSourceFactories
                    .FindMatches(MemberMapperData)
                    .Project(MemberMapperData, (md, dsf) => dsf.Create(md))
                    .ToArray();
            }
        }

        public IDataSource MatchingSourceMemberDataSource
            => _matchingSourceMemberDataSource ??= GetSourceMemberDataSource();

        private IDataSource GetSourceMemberDataSource()
        {
            if (BestSourceMemberMatch.IsUseable)
            {
                return GetFinalDataSource(
                    _bestSourceMemberMatch.CreateDataSource(),
                    _bestSourceMemberMatch.ContextMappingData);
            }

            return new AdHocDataSource(
                _bestSourceMemberMatch.SourceMember,
                Constants.EmptyExpression);
        }

        public SourceMemberMatch BestSourceMemberMatch =>
            _bestSourceMemberMatch ??= SourceMemberMatcher.GetMatchFor(SourceMemberMatchContext);

        private SourceMemberMatchContext SourceMemberMatchContext =>
            (_sourceMemberMatchContext != null)
                ? _sourceMemberMatchContext.With(MemberMappingData)
                : _sourceMemberMatchContext = new SourceMemberMatchContext(MemberMappingData);

        public bool UseSourceMemberDataSource()
        {
            return _useSourceMemberDataSource ??=
                    BestSourceMemberMatch.IsUseable &&
                   !ConfiguredDataSources.Any(MatchingSourceMemberDataSource, (msmds, cds) => cds.IsSameAs(msmds));
        }

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

        public DataSourceFindContext With(IChildMemberMappingData memberMappingData)
        {
            MemberMappingData = memberMappingData;
            _configuredDataSources = null;
            _sourceMemberMatchContext = null;
            _bestSourceMemberMatch = null;
            _matchingSourceMemberDataSource = null;
            _useSourceMemberDataSource = null;
            DataSourceIndex = 0;
            StopFind = false;
            return this;
        }

        #region IDataSourceSetInfo Members

        IMappingContext IMappingContextOwner.MappingContext => MemberMappingData.Parent.MappingContext;

        IMemberMapperData IDataSourceSetInfo.MapperData => MemberMapperData;

        #endregion
    }
}