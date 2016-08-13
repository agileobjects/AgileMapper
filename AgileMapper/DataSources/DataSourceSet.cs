namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using ObjectPopulation;

    internal class DataSourceSet : IEnumerable<IDataSource>
    {
        private readonly IEnumerable<IDataSource> _dataSources;
        private readonly List<ParameterExpression> _variables;
        private readonly List<IObjectMapper> _inlineObjectMappers;

        public DataSourceSet(params IDataSource[] dataSources)
        {
            _dataSources = dataSources;
            _variables = new List<ParameterExpression>();
            _inlineObjectMappers = new List<IObjectMapper>();
            None = dataSources.Length == 0;

            if (None)
            {
                return;
            }

            Value = GetValue(dataSources);

            foreach (var dataSource in dataSources)
            {
                HasValue = HasValue || dataSource.IsValid;
                _variables.AddRange(dataSource.Variables);
                _inlineObjectMappers.AddRange(dataSource.InlineObjectMappers);
            }
        }

        #region Setup

        private static Expression GetValue(IList<IDataSource> dataSources)
        {
            if (dataSources.Count == 1)
            {
                return dataSources[0].Value;
            }

            return dataSources
                .Reverse()
                .Skip(1)
                .Aggregate(
                    dataSources.Last().Value,
                    (valueSoFar, dataSource) => dataSource.GetValueOption(valueSoFar));
        }

        #endregion

        public bool None { get; }

        public bool HasValue { get; }

        public Expression Value { get; }

        public IEnumerable<ParameterExpression> Variables => _variables;

        public IEnumerable<IObjectMapper> InlineObjectMappers => _inlineObjectMappers;

        public IEnumerator<IDataSource> GetEnumerator() => _dataSources.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}