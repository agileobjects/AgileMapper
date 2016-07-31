namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class ConfiguredDataSource : DataSourceBase, IConfiguredDataSource
    {
        private readonly string _originalValueString;

        public ConfiguredDataSource(
            int dataSourceIndex,
            Expression value,
            Expression configuredCondition,
            IMemberMappingContext context)
            : base(
                  new ConfiguredSourceMember(value, context),
                  GetConvertedValue(dataSourceIndex, value, context),
                  context)
        {
            _originalValueString = GetConvertedValue(dataSourceIndex, value, context).ToString();

            if (configuredCondition == null)
            {
                Condition = base.Condition;
                return;
            }

            configuredCondition = Process(configuredCondition, context);

            Condition = (base.Condition != null)
                ? Expression.AndAlso(base.Condition, configuredCondition)
                : configuredCondition;
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
            => otherDataSource.Value.ToString() == _originalValueString;
    }
}