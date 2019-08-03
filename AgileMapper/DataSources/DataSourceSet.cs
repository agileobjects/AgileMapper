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
        private Expression _value;

        public DataSourceSet(IMemberMapperData mapperData, params IDataSource[] dataSources)
        {
            MapperData = mapperData;
            _dataSources = dataSources;
            None = dataSources.Length == 0;

            if (None)
            {
                Variables = Enumerable<ParameterExpression>.EmptyArray;
                return;
            }

            var variables = default(List<ParameterExpression>);

            for (var i = 0; i < dataSources.Length;)
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
                    if (variables == null)
                    {
                        variables = new List<ParameterExpression>();
                    }

                    variables.AddRange(dataSource.Variables);
                }

                if (dataSource.SourceMemberTypeTest != null)
                {
                    SourceMemberTypeTest = dataSource.SourceMemberTypeTest;
                }
            }

            Variables = (variables != null)
                ? (IList<ParameterExpression>)variables
                : Enumerable<ParameterExpression>.EmptyArray;
        }

        public IMemberMapperData MapperData { get; }

        public bool None { get; }

        public bool HasValue { get; }

        public bool IsConditional { get; }

        public Expression SourceMemberTypeTest { get; }

        public IList<ParameterExpression> Variables { get; }

        public IDataSource this[int index] => _dataSources[index];

        public int Count => _dataSources.Count;

        public Expression ValueExpression => _value ?? (_value = BuildValueExpression());

        private Expression BuildValueExpression()
        {
            var value = default(Expression);

            for (var i = _dataSources.Count - 1; i >= 0;)
            {
                var isFirstDataSource = value == default(Expression);
                var dataSource = _dataSources[i--];

                var dataSourceValue = dataSource.IsConditional
                    ? Expression.Condition(
                        dataSource.Condition,
                        isFirstDataSource
                            ? dataSource.Value
                            : dataSource.Value.GetConversionTo(value.Type),
                        isFirstDataSource
                            ? dataSource.Value.Type.ToDefaultExpression()
                            : value)
                    : dataSource.Value;

                value = dataSource.AddPreConditionIfNecessary(dataSourceValue);
            }

            return value;
        }

        public Expression GetFinalValueOrNull()
        {
            var finalDataSource = _dataSources.Last();
            var finalValue = finalDataSource.Value;

            if (finalDataSource.IsConditional || _dataSources.HasOne())
            {
                return finalValue;
            }

            if (finalValue.NodeType == ExpressionType.Coalesce)
            {
                // Coalesce between the existing target member value and the fallback:
                return ((BinaryExpression)finalValue).Right;
            }

            var targetMemberAccess = MapperData.GetTargetMemberAccess();

            if (ExpressionEvaluation.AreEqual(finalValue, targetMemberAccess))
            {
                return null;
            }

            return finalValue;
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