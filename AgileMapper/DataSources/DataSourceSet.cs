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

    internal class DataSourceSet : IEnumerable<IDataSource>
    {
        private readonly IList<IDataSource> _dataSources;
        private readonly Func<IList<IDataSource>, IMemberMapperData, Expression> _valueBuilder;
        private Expression _value;

        private DataSourceSet(
            IDataSource dataSource, 
            IMemberMapperData mapperData,
            Func<IList<IDataSource>, IMemberMapperData, Expression> valueBuilder)
        {
            _dataSources = new[] { dataSource };
            MapperData = mapperData;
            _valueBuilder = valueBuilder ?? ValueExpressionBuilders.SingleDataSource;
            HasValue = dataSource.IsValid;
            IsConditional = dataSource.IsConditional;
            Variables = dataSource.Variables;
            SourceMemberTypeTest = dataSource.SourceMemberTypeTest;
        }

        private DataSourceSet(
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

        #region Factory Methods

        public static DataSourceSet For(
            IDataSource dataSource, 
            IMemberMapperData mapperData,
            Func<IList<IDataSource>, IMemberMapperData, Expression> valueBuilder = null)
        {
            return new DataSourceSet(dataSource, mapperData, valueBuilder);
        }

        public static DataSourceSet For(
            IList<IDataSource> dataSources,
            IMemberMapperData mapperData,
            Func<IList<IDataSource>, IMemberMapperData, Expression> valueBuilder = null)
        {
            return dataSources.HasOne()
                ? For(dataSources.First(), mapperData, valueBuilder)
                : new DataSourceSet(dataSources, mapperData, valueBuilder);
        }

        #endregion

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