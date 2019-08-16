namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;

    internal static class ValueExpressionBuilders
    {
        public static Expression SingleDataSource(IList<IDataSource> dataSources, IMemberMapperData mapperData)
        {
            var dataSource = dataSources[0];

            var value = dataSource.IsConditional
                ? Expression.Condition(
                    dataSource.Condition,
                    dataSource.Value,
                    dataSource.Value.Type.ToDefaultExpression())
                : dataSource.Value;

            return dataSource.AddPreConditionIfNecessary(value);
        }

        public static Expression ConditionTree(IList<IDataSource> dataSources, IMemberMapperData mapperData)
        {
            var value = default(Expression);

            for (var i = dataSources.Count - 1; i >= 0;)
            {
                var isFirstDataSource = value == default(Expression);
                var dataSource = dataSources[i--];

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

        public static Expression SequentialValues(IList<IDataSource> dataSources, IMemberMapperData mapperData)
        {
            var mappingExpressions = new List<Expression>();

            foreach (var dataSource in dataSources)
            {
                var mapping = dataSource.Finalise(dataSource.Value);

                if (dataSource.IsConditional)
                {
                    mapping = Expression.IfThen(dataSource.Condition, mapping);
                }

                mapping = dataSource.AddPreConditionIfNecessary(mapping);

                mappingExpressions.Add(mapping);
            }

            return Expression.Block(mappingExpressions);
        }
    }
}