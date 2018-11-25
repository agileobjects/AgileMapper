namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections;
    using System.Collections.Generic;
    using Extensions.Internal;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class DataSourceSet : IEnumerable<IDataSource>
    {
        private readonly IList<IDataSource> _dataSources;
        private readonly IList<ParameterExpression> _variables;
        private Expression _value;

        public DataSourceSet(IMemberMapperData mapperData, params IDataSource[] dataSources)
        {
            MapperData = mapperData;
            _dataSources = dataSources;
            None = dataSources.Length == 0;

            if (None)
            {
                _variables = Enumerable<ParameterExpression>.EmptyArray;
                return;
            }

            var variables = new List<ParameterExpression>();

            for (var i = 0; i < dataSources.Length; )
            {
                var dataSource = dataSources[i++];

                if (dataSource.IsValid)
                {
                    HasValue = true;
                }

                if (dataSource.IsConditional)
                {
                    IsConditional = true;
                }

                if (dataSource.Variables.Any())
                {
                    variables.AddRange(dataSource.Variables);
                }

                if (dataSource.SourceMemberTypeTest != null)
                {
                    SourceMemberTypeTest = dataSource.SourceMemberTypeTest;
                }
            }

            _variables = variables;
        }

        public IMemberMapperData MapperData { get; }

        public bool None { get; }

        public bool HasValue { get; }

        public bool IsConditional { get; }

        public Expression SourceMemberTypeTest { get; }

        public ICollection<ParameterExpression> Variables => _variables;

        public IDataSource this[int index] => _dataSources[index];

        public Expression ValueExpression => _value ?? (_value = BuildValueExpression());

        private Expression BuildValueExpression()
        {
            var value = default(Expression);

            for (var i = _dataSources.Count - 1; i >= 0; --i)
            {
                var dataSource = _dataSources[i];

                value = dataSource.AddPreConditionIfNecessary(value == default(Expression)
                    ? dataSource.Value
                    : Expression.Condition(
                        dataSource.Condition,
                        dataSource.Value.GetConversionTo(value.Type),
                        value));
            }

            return value;
        }

        public Expression GetPopulationExpression()
        {
            var fallbackValue = GetFallbackValueOrNull();
            var excludeFallback = fallbackValue == null;

            Expression population = null;

            for (var i = _dataSources.Count - 1; i >= 0; --i)
            {
                var dataSource = _dataSources[i];

                if (i == _dataSources.Count - 1)
                {
                    if (excludeFallback)
                    {
                        continue;
                    }

                    population = MapperData.GetTargetMemberPopulation(fallbackValue);

                    if (dataSource.IsConditional)
                    {
                        population = dataSource.AddCondition(population);
                    }

                    population = dataSource.AddPreCondition(population);
                    continue;
                }

                var memberPopulation = MapperData.GetTargetMemberPopulation(dataSource.Value);

                population = dataSource.AddCondition(memberPopulation, population);
                population = dataSource.AddPreCondition(population);
            }

            return population;
        }

        private Expression GetFallbackValueOrNull()
        {
            var finalDataSource = _dataSources.Last();
            var fallbackValue = finalDataSource.Value;

            if (finalDataSource.IsConditional || _dataSources.HasOne())
            {
                return fallbackValue;
            }

            if (fallbackValue.NodeType == ExpressionType.Coalesce)
            {
                return ((BinaryExpression)fallbackValue).Right;
            }

            var targetMemberAccess = MapperData.GetTargetMemberAccess();

            if (ExpressionEvaluation.AreEqual(fallbackValue, targetMemberAccess))
            {
                return null;
            }

            return fallbackValue;
        }

        #region IEnumerable<IDataSource> Members

        public IEnumerator<IDataSource> GetEnumerator() => _dataSources.GetEnumerator();

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}