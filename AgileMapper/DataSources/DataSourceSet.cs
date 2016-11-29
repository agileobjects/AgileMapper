namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions;

    internal class DataSourceSet : IEnumerable<IDataSource>
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

            Value = dataSources.Chain(
                dataSource => dataSource.Value,
                (dataSource, valueSoFar) =>
                    Expression.Condition(dataSource.Condition, dataSource.Value, valueSoFar));

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

        public bool None { get; }

        public bool HasValue { get; }

        public Expression SourceMemberTypeTest { get; }

        public Expression Value { get; }

        public IEnumerable<ParameterExpression> Variables => _variables;

        public IDataSource this[int index] => _dataSources[index];

        #region IEnumerable<IDataSource> Members

        public IEnumerator<IDataSource> GetEnumerator() => _dataSources.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}