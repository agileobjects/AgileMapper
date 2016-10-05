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
            MemberMapperData data)
            : this(
                  new ConfiguredSourceMember(value, data),
                  configuredCondition,
                  GetConvertedValue(dataSourceIndex, value, data),
                  data)
        {
        }

        private ConfiguredDataSource(
            IQualifiedMember sourceMember,
            Expression configuredCondition,
            Expression convertedValue,
            MemberMapperData data)
            : base(sourceMember, convertedValue, data)
        {
            _originalValueString = convertedValue.ToString();

            Expression condition;

            if (configuredCondition != null)
            {
                configuredCondition = Process(configuredCondition, data);

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

        private static Expression GetConvertedValue(int dataSourceIndex, Expression value, MemberMapperData data)
        {
            if (data.TargetMember.IsComplex && (data.TargetMember.Type.GetAssembly() != typeof(string).GetAssembly()))
            {
                return data.GetMapCall(value, dataSourceIndex);
            }

            var convertedValue = data.MapperContext.ValueConverters.GetConversion(value, data.TargetMember.Type);

            return convertedValue;
        }

        private static Expression Process(Expression configuredCondition, MemberMapperData data)
        {
            var conditionNestedAccessesChecks = data
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