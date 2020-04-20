namespace AgileObjects.AgileMapper.Members.Population
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using DataSources;
    using Extensions.Internal;

    internal class MemberPopulator : IMemberPopulator
    {
        private readonly IDataSourceSet _dataSources;

        public MemberPopulator(
            IDataSourceSet dataSources,
            IMemberMapperData mapperData,
            Expression populateCondition = null)
        {
            _dataSources = dataSources;
            MapperData = mapperData;
            PopulateCondition = populateCondition;
        }

        public IMemberMapperData MapperData { get; }

        public bool CanPopulate => true;

        public Expression PopulateCondition { get; }

        public Expression GetPopulation()
        {
            var populationGuard = MapperData
                .RuleSet
                .PopulationGuardFactory
                .Invoke(this);

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
                bindingValue = bindingValue.ToIfFalseDefaultCondition(populationGuard, MapperData);
            }

            return MapperData.GetTargetMemberPopulation(bindingValue);
        }

        private Expression GetReadOnlyMemberPopulation()
        {
            var targetMemberAccess = MapperData.GetTargetMemberAccess();
            var targetMemberNotNull = targetMemberAccess.GetIsNotDefaultComparison();

            return Expression.IfThen(targetMemberNotNull, _dataSources.BuildValue());
        }

        public Expression GetPopulationExpression()
        {
            var dataSourcesCount = _dataSources.Count;

            if (dataSourcesCount == 1)
            {
                return GetPopulation(_dataSources[0]);
            }

            var population = default(Expression);
            var nextDataSource = default(IDataSource);

            for (var i = _dataSources.Count; ;)
            {
                --i;
                var dataSource = _dataSources[i];
                population = GetPopulation(dataSource, population, nextDataSource);

                if (i == 0)
                {
                    return population;
                }

                nextDataSource = dataSource;
            }
        }

        private Expression GetPopulation(
            IDataSource dataSource,
            Expression populationSoFar = null,
            IDataSource nextDataSource = null)
        {
            return dataSource.FinalisePopulationBranch(
                populationSoFar,
                nextDataSource,
                MapperData);
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
            => $"{MapperData.TargetMember} ({_dataSources.Count} data source(s))";
    }
}