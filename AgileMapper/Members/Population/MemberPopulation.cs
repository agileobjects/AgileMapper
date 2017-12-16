namespace AgileObjects.AgileMapper.Members.Population
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Configuration;
    using DataSources;
    using Extensions.Internal;
    using ReadableExpressions;

    internal class MemberPopulation : IMemberPopulation
    {
        private readonly DataSourceSet _dataSources;
        private readonly Expression _populateCondition;

        private MemberPopulation(
            IMemberMapperData mapperData,
            DataSourceSet dataSources,
            Expression populateCondition = null)
        {
            MapperData = mapperData;
            _dataSources = dataSources;
            _populateCondition = populateCondition;
        }

        #region Factory Methods

        public static IMemberPopulation WithRegistration(
            IChildMemberMappingData mappingData,
            DataSourceSet dataSources,
            Expression populateCondition)
        {
            var memberPopulation = WithoutRegistration(mappingData, dataSources, populateCondition);

            memberPopulation.MapperData.RegisterTargetMemberDataSourcesIfRequired(dataSources);

            return memberPopulation;
        }

        public static IMemberPopulation WithoutRegistration(
            IChildMemberMappingData mappingData,
            DataSourceSet dataSources,
            Expression populateCondition = null)
        {
            if (!dataSources.None)
            {
                populateCondition = GetPopulateCondition(populateCondition, mappingData);
            }

            return new MemberPopulation(mappingData.MapperData, dataSources, populateCondition);
        }

        private static Expression GetPopulateCondition(Expression populateCondition, IChildMemberMappingData mappingData)
        {
            var populationGuard = mappingData.GetRuleSetPopulationGuardOrNull();

            if (populationGuard == null)
            {
                return populateCondition;
            }

            if (populateCondition == null)
            {
                return populationGuard;
            }

            return Expression.AndAlso(populateCondition, populationGuard);
        }

        public static IMemberPopulation Unmappable(IMemberMapperData mapperData, string reason)
            => CreateNullMemberPopulation(mapperData, targetMember => $"No way to populate {targetMember.Name} ({reason})");

        public static IMemberPopulation IgnoredMember(IMemberMapperData mapperData, ConfiguredIgnoredMember configuredIgnore)
            => CreateNullMemberPopulation(mapperData, configuredIgnore.GetIgnoreMessage);

        public static IMemberPopulation NoDataSource(IMemberMapperData mapperData)
        {
            var noDataSources = CreateNullDataSourceSet(mapperData, GetNoDataSourceMessage);

            mapperData.RegisterTargetMemberDataSourcesIfRequired(noDataSources);

            return new MemberPopulation(mapperData, noDataSources);
        }

        private static string GetNoDataSourceMessage(QualifiedMember targetMember)
        {
            return targetMember.IsSimple
                ? "No data source for " + targetMember.Name
                : $"No data source for {targetMember.Name} or any of its child members";
        }

        private static MemberPopulation CreateNullMemberPopulation(
            IMemberMapperData mapperData,
            Func<QualifiedMember, string> commentFactory)
        {
            return new MemberPopulation(mapperData, CreateNullDataSourceSet(mapperData, commentFactory));
        }

        private static DataSourceSet CreateNullDataSourceSet(
            IBasicMapperData mapperData,
            Func<QualifiedMember, string> commentFactory)
        {
            return new DataSourceSet(
                new NullDataSource(
                    ReadableExpression.Comment(commentFactory.Invoke(mapperData.TargetMember))));
        }

        #endregion

        public IMemberMapperData MapperData { get; }

        public bool IsSuccessful => _dataSources.HasValue;

        public Expression GetPopulation()
        {
            if (!IsSuccessful)
            {
                return _dataSources.GetValueExpression();
            }

            var population = MapperData.Context.IsPartOfUserStructMapping
                ? GetBinding()
                : MapperData.TargetMember.IsReadOnly
                    ? GetReadOnlyMemberPopulation()
                    : _dataSources.GetPopulationExpression(MapperData);

            if (_dataSources.Variables.Any())
            {
                population = Expression.Block(_dataSources.Variables, population);
            }

            if (_populateCondition != null)
            {
                population = Expression.IfThen(_populateCondition, population);
            }

            return population;
        }

        private Expression GetBinding()
        {
            var bindingValue = _dataSources.GetValueExpression();
            var binding = MapperData.GetTargetMemberPopulation(bindingValue);

            return binding;
        }

        private Expression GetReadOnlyMemberPopulation()
        {
            var dataSourcesValue = _dataSources.GetValueExpression();
            var targetMemberAccess = MapperData.GetTargetMemberAccess();
            var targetMemberNotNull = targetMemberAccess.GetIsNotDefaultComparison();

            return Expression.IfThen(targetMemberNotNull, dataSourcesValue);
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString()
            => $"{MapperData.TargetMember} ({_dataSources.Count()} data source(s))";
    }
}