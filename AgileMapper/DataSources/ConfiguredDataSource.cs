namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class ConfiguredDataSource : DataSourceBase, IConfiguredDataSource
    {
        private readonly string _conditionString;
        private readonly string _originalValueString;

        public ConfiguredDataSource(
            int dataSourceIndex,
            Expression configuredCondition,
            Expression value,
            IMemberMappingContext context)
            : this(
                  new ConfiguredSourceMember(value, context),
                  configuredCondition,
                  GetConvertedValue(dataSourceIndex, value, context),
                  context)
        {
        }

        private ConfiguredDataSource(
            IQualifiedMember sourceMember,
            Expression configuredCondition,
            Expression convertedValue,
            IMemberMappingContext context)
            : base(sourceMember, convertedValue, context)
        {
            _originalValueString = convertedValue.ToString();

            Expression condition;

            if (configuredCondition != null)
            {
                configuredCondition = Process(configuredCondition, context);

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

        private static Expression GetConvertedValue(int dataSourceIndex, Expression value, IMemberMappingContext context)
        {
            if (context.TargetMember.IsComplex && (context.TargetMember.Type.Assembly != typeof(string).Assembly))
            {
                return context.GetMapCall(value, dataSourceIndex);
            }

            var convertedValue = context.MapperContext.ValueConverters.GetConversion(value, context.TargetMember.Type);

            return convertedValue;
        }

        private static Expression Process(Expression configuredCondition, IMemberMappingContext context)
        {
            var conditionNestedAccessesChecks = context
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