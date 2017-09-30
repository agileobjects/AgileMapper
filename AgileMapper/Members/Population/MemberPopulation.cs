namespace AgileObjects.AgileMapper.Members.Population
{
    using System;
    using System.Linq;
#if !NET_STANDARD
    using System.Diagnostics.CodeAnalysis;
#endif
    using System.Linq.Expressions;
    using Configuration;
    using DataSources;
    using Extensions;
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
            Expression populateCondition = null)
        {
            var memberPopulation = WithoutRegistration(mappingData, dataSources, populateCondition);
            var mapperData = memberPopulation.MapperData;

            mapperData.Parent.RegisterTargetMemberDataSourcesIfRequired(mapperData.TargetMember, dataSources);

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

        public static IMemberPopulation IgnoredMember(IMemberMapperData mapperData, ConfiguredIgnoredMember configuredIgnore)
            => CreateNullMemberPopulation(mapperData, configuredIgnore.GetIgnoreMessage);

        public static IMemberPopulation NoDataSource(IMemberMapperData mapperData)
            => CreateNullMemberPopulation(mapperData, targetMember => "No data source for " + targetMember.Name);

        private static IMemberPopulation CreateNullMemberPopulation(
            IMemberMapperData mapperData,
            Func<IQualifiedMember, string> commentFactory)
        {
            return new MemberPopulation(
                mapperData,
                new DataSourceSet(
                    new NullDataSource(
                        ReadableExpression.Comment(commentFactory.Invoke(mapperData.TargetMember)))));
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

            var population = MapperData.TargetMember.IsReadOnly
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

        private Expression GetReadOnlyMemberPopulation()
        {
            var dataSourcesValue = _dataSources.GetValueExpression();
            var targetMemberAccess = MapperData.GetTargetMemberAccess();
            var targetMemberNotNull = targetMemberAccess.GetIsNotDefaultComparison();

            return Expression.IfThen(targetMemberNotNull, dataSourcesValue);
        }

        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString()
            => $"{MapperData.TargetMember} ({_dataSources.Count()} data source(s))";
    }
}