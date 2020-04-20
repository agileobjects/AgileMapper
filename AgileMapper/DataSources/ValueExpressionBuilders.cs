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
            => SingleDataSource(dataSources.First(), mapperData);

        public static Expression SingleDataSource(IDataSource dataSource, IMemberMapperData mapperData)
        {
            var value = dataSource.IsConditional
                ? dataSource.Value.ToIfFalseDefaultCondition(dataSource.Condition, mapperData)
                : dataSource.Value;

            return dataSource.AddSourceCondition(value);
        }

        public static Expression ValueTree(IList<IDataSource> dataSources, IMemberMapperData mapperData)
        {
            var value = SingleDataSource(dataSources.Last(), mapperData);

            for (var i = dataSources.Count - 2; i >= 0;)
            {
                var dataSource = dataSources[i];
                --i;

                var dataSourceValue = dataSource.IsConditional
                    ? Expression.Condition(
                        dataSource.Condition,
                        dataSource.Value.GetConversionTo(value.Type),
                        value)
                    : dataSource.Value;

                value = dataSource.AddSourceCondition(dataSourceValue);
            }

            return value;
        }

        public static Expression ValueSequence(IList<IDataSource> dataSources, IMemberMapperData mapperData)
        {
            if (dataSources.HasOne())
            {
                return dataSources.First().GetValueSequenceValue();
            }

            var mappingExpressions = dataSources
                .ProjectToArray(dataSource => dataSource.GetValueSequenceValue());

            return Expression.Block(mappingExpressions);
        }

        private static Expression GetValueSequenceValue(this IDataSource dataSource)
        {
            var mapping = dataSource.Value;

            if (dataSource.IsConditional)
            {
                mapping = Expression.IfThen(dataSource.Condition, mapping);
            }

            mapping = dataSource.AddSourceCondition(mapping);

            if (dataSource.Variables.Any())
            {
                mapping = Expression.Block(dataSource.Variables, mapping);
            }

            return mapping;
        }
    }
}