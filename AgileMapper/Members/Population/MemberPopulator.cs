namespace AgileObjects.AgileMapper.Members.Population
{
    using System;
    using System.Linq;
    using Configuration;
    using DataSources;
    using ReadableExpressions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class MemberPopulator : IMemberPopulationContext, IMemberPopulator
    {
        private MemberPopulator(DataSourceSet dataSources, Expression populateCondition = null)
        {
            DataSources = dataSources;
            PopulateCondition = populateCondition;
        }

        #region Factory Methods

        public static IMemberPopulator WithRegistration(
            IChildMemberMappingData mappingData,
            DataSourceSet dataSources,
            Expression populateCondition)
        {
            var memberPopulation = WithoutRegistration(mappingData, dataSources, populateCondition);

            memberPopulation.MapperData.RegisterTargetMemberDataSourcesIfRequired(dataSources);

            return memberPopulation;
        }

        public static IMemberPopulator WithoutRegistration(
            IChildMemberMappingData mappingData,
            DataSourceSet dataSources,
            Expression populateCondition = null)
        {
            return new MemberPopulator(dataSources, populateCondition);
        }

        public static IMemberPopulator Unmappable(IMemberMapperData mapperData, string reason)
            => CreateNullMemberPopulation(mapperData, targetMember => $"No way to populate {targetMember.Name} ({reason})");

        public static IMemberPopulator IgnoredMember(IMemberMapperData mapperData, ConfiguredIgnoredMember configuredIgnore)
            => CreateNullMemberPopulation(mapperData, configuredIgnore.GetIgnoreMessage);

        public static IMemberPopulator NoDataSource(IMemberMapperData mapperData)
        {
            var noDataSources = CreateNullDataSourceSet(mapperData, GetNoDataSourceMessage);

            mapperData.RegisterTargetMemberDataSourcesIfRequired(noDataSources);

            return new MemberPopulator(noDataSources);
        }

        private static string GetNoDataSourceMessage(QualifiedMember targetMember)
        {
            return targetMember.IsSimple
                ? "No data source for " + targetMember.Name
                : $"No data source for {targetMember.Name} or any of its child members";
        }

        private static MemberPopulator CreateNullMemberPopulation(
            IMemberMapperData mapperData,
            Func<QualifiedMember, string> commentFactory)
        {
            return new MemberPopulator(CreateNullDataSourceSet(mapperData, commentFactory));
        }

        private static DataSourceSet CreateNullDataSourceSet(
            IMemberMapperData mapperData,
            Func<QualifiedMember, string> commentFactory)
        {
            return new DataSourceSet(
                mapperData,
                new NullDataSource(
                    ReadableExpression.Comment(commentFactory.Invoke(mapperData.TargetMember))));
        }

        #endregion

        public IMemberMapperData MapperData => DataSources.MapperData;

        public bool IsSuccessful => CanPopulate;

        public bool CanPopulate => DataSources.HasValue;

        public DataSourceSet DataSources { get; }

        public Expression PopulateCondition { get; }

        public Expression GetPopulation()
            => MapperData.RuleSet.PopulationFactory.GetPopulation(this);

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString()
            => $"{MapperData.TargetMember} ({DataSources.Count()} data source(s))";
    }
}