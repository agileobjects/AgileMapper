namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;

    internal static class DataSourceSet
    {
        #region Factory Methods

        public static IDataSourceSet For(
            IDataSource dataSource,
            IMemberMapperData mapperData,
            Func<IList<IDataSource>, IMemberMapperData, Expression> valueBuilder = null)
        {
            if (mapperData.MapperContext.UserConfigurations.HasSourceValueFilters)
            {
                dataSource = dataSource.WithFilter(mapperData);
            }

            return new SingleValueDataSourceSet(dataSource, mapperData, valueBuilder);
        }

        public static IDataSourceSet For(
            IList<IDataSource> dataSources,
            IMemberMapperData mapperData,
            Func<IList<IDataSource>, IMemberMapperData, Expression> valueBuilder = null)
        {
            switch (dataSources.Count)
            {
                case 0:
                    return EmptyDataSourceSet.Instance;

                case 1:
                    return For(dataSources.First(), mapperData, valueBuilder);

                default:
                    if (TryAdjustToSingleUseableDataSource(ref dataSources, mapperData))
                    {
                        goto case 1;
                    }

                    if (mapperData.MapperContext.UserConfigurations.HasSourceValueFilters)
                    {
                        dataSources = dataSources.WithFilters(mapperData);
                    }

                    return new MultipleValueDataSourceSet(dataSources, mapperData, valueBuilder);
            }
        }

        private static bool TryAdjustToSingleUseableDataSource(
            ref IList<IDataSource> dataSources,
            IMemberMapperData mapperData)
        {
            var finalDataSource = dataSources.Last();

            if (!finalDataSource.IsFallback)
            {
                return false;
            }

            var finalValue = finalDataSource.Value;

            if (finalValue.NodeType == ExpressionType.Coalesce)
            {
                // Coalesce between the existing target member value and the fallback:
                dataSources[dataSources.Count - 1] = new AdHocDataSource(
                    finalDataSource.SourceMember,
                  ((BinaryExpression)finalValue).Right,
                    finalDataSource.Condition,
                    finalDataSource.Variables);

                return false;
            }

            var targetMemberAccess = mapperData.GetTargetMemberAccess();

            if (!ExpressionEvaluation.AreEqual(finalValue, targetMemberAccess))
            {
                return false;
            }

            if (dataSources.Count == 2)
            {
                return true;
            }

            var dataSourcesWithoutFallback = new IDataSource[dataSources.Count - 1];
            dataSourcesWithoutFallback.CopyFrom(dataSources);
            dataSources = dataSourcesWithoutFallback;
            return false;
        }

        #endregion

        private class SingleValueDataSourceSet : IDataSourceSet
        {
            private readonly IDataSource _dataSource;
            private readonly IMemberMapperData _mapperData;
            private readonly Func<IDataSource, IMemberMapperData, Expression> _valueBuilder;
            private Expression _value;

            public SingleValueDataSourceSet(
                IDataSource dataSource,
                IMemberMapperData mapperData,
                Func<IList<IDataSource>, IMemberMapperData, Expression> valueBuilder)
            {
                _dataSource = dataSource;
                _mapperData = mapperData;

                if (valueBuilder == null)
                {
                    _valueBuilder = ValueExpressionBuilders.SingleDataSource;
                }
                else
                {
                    _valueBuilder = (ds, md) => valueBuilder.Invoke(new[] { ds }, md);
                }
            }

            public bool None => false;

            public bool HasValue => _dataSource.IsValid;

            public bool IsConditional => _dataSource.IsConditional;

            public Expression SourceMemberTypeTest => _dataSource.SourceMemberTypeTest;

            public IList<ParameterExpression> Variables => _dataSource.Variables;

            public IDataSource this[int index] => _dataSource;

            public int Count => 1;

            public Expression BuildValue()
                => _value ??= _valueBuilder.Invoke(_dataSource, _mapperData);
        }

        private class MultipleValueDataSourceSet : IDataSourceSet
        {
            private readonly IList<IDataSource> _dataSources;
            private readonly IMemberMapperData _mapperData;
            private readonly Func<IList<IDataSource>, IMemberMapperData, Expression> _valueBuilder;
            private Expression _value;

            public MultipleValueDataSourceSet(
                IList<IDataSource> dataSources,
                IMemberMapperData mapperData,
                Func<IList<IDataSource>, IMemberMapperData, Expression> valueBuilder)
            {
                _dataSources = dataSources;
                _mapperData = mapperData;
                _valueBuilder = valueBuilder ?? ValueExpressionBuilders.ConditionTree;

                var dataSourcesCount = dataSources.Count;
                var variables = default(List<ParameterExpression>);

                for (var i = 0; i < dataSourcesCount; ++i)
                {
                    var dataSource = dataSources[i];

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

            public bool None => false;

            public bool HasValue { get; }

            public bool IsConditional { get; }

            public Expression SourceMemberTypeTest { get; }

            public IList<ParameterExpression> Variables { get; }

            public IDataSource this[int index] => _dataSources[index];

            public int Count => _dataSources.Count;

            public Expression BuildValue()
                => _value ?? (_value = _valueBuilder.Invoke(_dataSources, _mapperData));
        }
    }
}