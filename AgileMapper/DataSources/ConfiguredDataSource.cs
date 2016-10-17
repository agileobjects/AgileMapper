namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using ReadableExpressions.Extensions;

    internal class ConfiguredDataSource : DataSourceBase, IConfiguredDataSource
    {
        private readonly string _conditionString;
        private readonly string _originalValueString;

        public ConfiguredDataSource(
            int dataSourceIndex,
            Expression configuredCondition,
            Expression value,
            IMemberMapperData mapperData)
            : this(
                  new ConfiguredSourceMember(value, mapperData),
                  configuredCondition,
                  GetConvertedValue(dataSourceIndex, value, mapperData),
                  mapperData)
        {
        }

        private ConfiguredDataSource(
            IQualifiedMember sourceMember,
            Expression configuredCondition,
            Expression convertedValue,
            IMemberMapperData mapperData)
            : base(sourceMember, convertedValue, mapperData)
        {
            _originalValueString = convertedValue.ToString();

            Expression condition;

            if (configuredCondition != null)
            {
                configuredCondition = Process(configuredCondition, mapperData);

                condition = (base.Condition != null)
                    ? Expression.AndAlso(base.Condition, configuredCondition)
                    : configuredCondition;
            }
            else
            {
                condition = base.Condition;
            }

            if (condition == null)
            {
                return;
            }

            Condition = condition;
            _conditionString = condition.ToString();
        }

        #region Setup

        private static Expression GetConvertedValue(int dataSourceIndex, Expression value, IMemberMapperData mapperData)
        {
            if (mapperData.TargetMember.IsComplex && (mapperData.TargetMember.Type.GetAssembly() != typeof(string).GetAssembly()))
            {
                return mapperData.GetMapCall(value, dataSourceIndex);
            }

            var convertedValue = mapperData.MapperContext.ValueConverters.GetConversion(value, mapperData.TargetMember.Type);

            return convertedValue;
        }

        private static Expression Process(Expression configuredCondition, IMemberMapperData mapperData)
        {
            var conditionNestedAccessesChecks = mapperData
                .GetNestedAccessesIn(configuredCondition)
                .GetIsNotDefaultComparisonsOrNull();

            if (conditionNestedAccessesChecks == null)
            {
                return configuredCondition;
            }

            var checkedConfiguredCondition = Expression.AndAlso(conditionNestedAccessesChecks, configuredCondition);

            return checkedConfiguredCondition;
        }

        #endregion

        public override Expression Condition { get; }

        public bool IsSameAs(IDataSource otherDataSource)
            => (otherDataSource.IsConditional && IsConditional && otherDataSource.Condition.ToString() == _conditionString) ||
               otherDataSource.Value.ToString() == _originalValueString;
    }
}