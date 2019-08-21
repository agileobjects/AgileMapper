namespace AgileObjects.AgileMapper.Members.Population
{
    using System;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Configuration;
    using DataSources;
    using Extensions.Internal;
    using ReadableExpressions;

    internal class MemberPopulator : IMemberPopulationContext, IMemberPopulator
    {
        private readonly DataSourceSet _dataSources;

        private MemberPopulator(DataSourceSet dataSources, Expression populateCondition = null)
        {
            _dataSources = dataSources;
            PopulateCondition = populateCondition;
        }

        #region Factory Methods

        public static IMemberPopulator WithRegistration(DataSourceSet dataSources, Expression populateCondition)
        {
            var memberPopulation = WithoutRegistration(dataSources, populateCondition);

            memberPopulation.MapperData.RegisterTargetMemberDataSourcesIfRequired(dataSources);

            return memberPopulation;
        }

        public static IMemberPopulator WithoutRegistration(DataSourceSet dataSources, Expression populateCondition = null)
            => new MemberPopulator(dataSources, populateCondition);

        public static IMemberPopulator Unmappable(IMemberMapperData mapperData, string reason)
            => CreateNullMemberPopulator(mapperData, targetMember => $"No way to populate {targetMember.Name} ({reason})");

        public static IMemberPopulator IgnoredMember(IMemberMapperData mapperData, ConfiguredIgnoredMember configuredIgnore)
            => CreateNullMemberPopulator(mapperData, configuredIgnore.GetIgnoreMessage);

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

        private static MemberPopulator CreateNullMemberPopulator(
            IMemberMapperData mapperData,
            Func<QualifiedMember, string> commentFactory)
        {
            return new MemberPopulator(CreateNullDataSourceSet(mapperData, commentFactory));
        }

        private static DataSourceSet CreateNullDataSourceSet(
            IMemberMapperData mapperData,
            Func<QualifiedMember, string> commentFactory)
        {
            return DataSourceSet.For(
                new NullDataSource(
                    ReadableExpression.Comment(commentFactory.Invoke(mapperData.TargetMember))),
                mapperData);
        }

        #endregion

        public IMemberMapperData MapperData => _dataSources.MapperData;

        public bool CanPopulate => _dataSources.HasValue;

        public Expression PopulateCondition { get; }

        public Expression GetPopulation()
        {
            if (!CanPopulate)
            {
                return _dataSources.BuildValue();
            }

            var populationGuard = MapperData
                .RuleSet
                .PopulationGuardFactory
                .GetPopulationGuard(this);

            var useSingleExpression = MapperData.UseMemberInitialisations();

            var population = useSingleExpression
                ? GetBinding(populationGuard)
                : MapperData.TargetMember.IsReadOnly
                    ? GetReadOnlyMemberPopulation()
                    : GetPopulationExpression();

            if (_dataSources.Variables.Any())
            {
                population = GetPopulationWithVariables(population);
            }

            if (useSingleExpression && MapperData.RuleSet.Settings.AllowGuardedBindings)
            {
                return population;
            }

            return (populationGuard != null)
                ? Expression.IfThen(populationGuard, population)
                : population;
        }

        private Expression GetBinding(Expression populationGuard)
        {
            var bindingValue = _dataSources.BuildValue();

            if (MapperData.RuleSet.Settings.AllowGuardedBindings && (populationGuard != null))
            {
                bindingValue = bindingValue.ToIfFalseDefaultCondition(populationGuard);
            }

            return MapperData.GetTargetMemberPopulation(bindingValue);
        }

        private Expression GetReadOnlyMemberPopulation()
        {
            var targetMemberAccess = MapperData.GetTargetMemberAccess();
            var targetMemberNotNull = targetMemberAccess.GetIsNotDefaultComparison();

            return Expression.IfThen(targetMemberNotNull, _dataSources.BuildValue());
        }

        private Expression GetPopulationExpression()
        {
            var finalValue = _dataSources.GetFinalValueOrNull();
            var excludeFinalValue = finalValue == null;
            var finalDataSourceIndex = _dataSources.Count - 1;

            Expression population = null;

            for (var i = finalDataSourceIndex; i >= 0; --i)
            {
                var dataSource = _dataSources[i];

                if (i == finalDataSourceIndex)
                {
                    if (excludeFinalValue)
                    {
                        continue;
                    }

                    population = MapperData.GetTargetMemberPopulation(finalValue);
                    population = dataSource.Finalise(population);
                    continue;
                }

                var memberPopulation = MapperData.GetTargetMemberPopulation(dataSource.Value);
                population = dataSource.Finalise(memberPopulation, population);
            }

            return population;
        }

        private Expression GetPopulationWithVariables(Expression population)
        {
            if (population.NodeType != ExpressionType.Block)
            {
                return Expression.Block(_dataSources.Variables, population);
            }

            var populationBlock = (BlockExpression)population;

            return Expression.Block(
                _dataSources.Variables.Append(populationBlock.Variables), 
                populationBlock.Expressions);
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