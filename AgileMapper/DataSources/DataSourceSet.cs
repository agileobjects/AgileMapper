namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal class DataSourceSet
    {
        private readonly IList<IDataSource> _dataSources;
        private readonly List<ParameterExpression> _variables;

        public DataSourceSet(params IDataSource[] dataSources)
        {
            _dataSources = dataSources;
            _variables = new List<ParameterExpression>();
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

                if (dataSource.SourceMemberTypeTest != null)
                {
                    SourceMemberTypeTest = dataSource.SourceMemberTypeTest;
                }
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

        public Expression SourceMemberTypeTest { get; }

        public Expression Value { get; }

        public IEnumerable<ParameterExpression> Variables => _variables;

        public IDataSource this[int index] => _dataSources[index];
    }
}