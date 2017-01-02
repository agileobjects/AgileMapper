namespace AgileObjects.AgileMapper.Members.Population
{
    using System;
    using System.Linq;
#if !NET_STANDARD
    using System.Diagnostics.CodeAnalysis;
#endif
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;
    using ReadableExpressions;

    internal class MemberPopulation : IMemberPopulation
    {
        private readonly DataSourceSet _dataSources;
        private readonly Expression _populateCondition;

        public MemberPopulation(
            IChildMemberMappingData mappingData,
            DataSourceSet dataSources,
            Expression populateCondition)
            : this(mappingData.MapperData, dataSources)
        {
            if (!dataSources.None)
            {
                _populateCondition = GetPopulateCondition(populateCondition, mappingData);
            }

            MapperData.Parent.RegisterTargetMemberDataSourcesIfRequired(MapperData.TargetMember, dataSources);
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

        private MemberPopulation(IMemberMapperData mapperData, DataSourceSet dataSources)
        {
            MapperData = mapperData;
            _dataSources = dataSources;
        }

        #region Factory Methods

        public static IMemberPopulation IgnoredMember(IMemberMapperData mapperData)
            => CreateNullMemberPopulation(mapperData, targetMember => targetMember.Name + " is ignored");

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

        public Expression SourceMemberTypeTest => _dataSources.SourceMemberTypeTest;

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
            var targetMemberAccess = MapperData.GetTargetMemberAccess();
            var targetMemberNotNull = targetMemberAccess.GetIsNotDefaultComparison();
            var dataSourcesValue = _dataSources.GetValueExpression();

            if (dataSourcesValue.NodeType != ExpressionType.Conditional)
            {
                return Expression.IfThen(targetMemberNotNull, dataSourcesValue);
            }

            var valueTernary = (ConditionalExpression)dataSourcesValue;
            var populationTest = Expression.AndAlso(targetMemberNotNull, valueTernary.Test);
            var population = Expression.IfThen(populationTest, valueTernary.IfTrue);

            return population;
        }

        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString()
            => MapperData.TargetMember + " (" + _dataSources.Count() + " data source(s))";
    }
}