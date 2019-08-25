namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Collections;
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
            return new SingleValueDataSourceSet(dataSource, mapperData, valueBuilder);
        }

        public static IDataSourceSet For(
            IList<IDataSource> dataSources,
            IMemberMapperData mapperData,
            Func<IList<IDataSource>, IMemberMapperData, Expression> valueBuilder = null)
        {
            return dataSources.HasOne()
                ? For(dataSources.First(), mapperData, valueBuilder)
                : new MultipleValueDataSourceSet(dataSources, mapperData, valueBuilder);
        }

        #endregion

        private class SingleValueDataSourceSet : IDataSourceSet
        {
            private readonly IDataSource _dataSource;
            private readonly Func<IDataSource, IMemberMapperData, Expression> _valueBuilder;
            private Expression _value;

            public SingleValueDataSourceSet(
                IDataSource dataSource,
                IMemberMapperData mapperData,
                Func<IList<IDataSource>, IMemberMapperData, Expression> valueBuilder)
            {
                _dataSource = dataSource;
                MapperData = mapperData;

                if (valueBuilder == null)
                {
                    _valueBuilder = ValueExpressionBuilders.SingleDataSource;
                }
                else
                {
                    _valueBuilder = (ds, md) => valueBuilder.Invoke(new[] { ds }, md);
                }
            }

            public IMemberMapperData MapperData { get; }

            public bool None => false;

            public bool HasValue => _dataSource.IsValid;

            public bool IsConditional => _dataSource.IsConditional;

            public Expression SourceMemberTypeTest => _dataSource.SourceMemberTypeTest;

            public IList<ParameterExpression> Variables => _dataSource.Variables;

            public IDataSource this[int index] => _dataSource;

            public int Count => 1;

            public Expression BuildValue()
                => _value ?? (_value = _valueBuilder.Invoke(_dataSource, MapperData));

            public Expression GetFinalValueOrNull() => _dataSource.Value;

            #region IEnumerable<IDataSource> Members

            #region ExcludeFromCodeCoverage
#if DEBUG
            [ExcludeFromCodeCoverage]
#endif
            #endregion
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public IEnumerator<IDataSource> GetEnumerator()
            {
                yield return _dataSource;
            }

            #endregion
        }

        private class MultipleValueDataSourceSet : IDataSourceSet
        {
            private readonly IList<IDataSource> _dataSources;
            private readonly Func<IList<IDataSource>, IMemberMapperData, Expression> _valueBuilder;
            private Expression _value;

            public MultipleValueDataSourceSet(
                IList<IDataSource> dataSources,
                IMemberMapperData mapperData,
                Func<IList<IDataSource>, IMemberMapperData, Expression> valueBuilder)
            {
                _dataSources = dataSources;
                MapperData = mapperData;
                None = dataSources.Count == 0;

                if (None)
                {
                    Variables = Enumerable<ParameterExpression>.EmptyArray;
                    return;
                }

                _valueBuilder = valueBuilder ?? ValueExpressionBuilders.ConditionTree;

                var variables = default(List<ParameterExpression>);

                for (var i = 0; i < dataSources.Count;)
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

            public Expression BuildValue()
                => _value ?? (_value = _valueBuilder.Invoke(_dataSources, MapperData));

            public Expression GetFinalValueOrNull()
            {
                var finalDataSource = _dataSources.Last();
                var finalValue = finalDataSource.Value;

                if (!finalDataSource.IsFallback)
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

            #region ExcludeFromCodeCoverage
#if DEBUG
            [ExcludeFromCodeCoverage]
#endif
            #endregion
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public IEnumerator<IDataSource> GetEnumerator() => _dataSources.GetEnumerator();

            #endregion
        }
    }
}