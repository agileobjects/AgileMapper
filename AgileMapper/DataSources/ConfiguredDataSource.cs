namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class ConfiguredDataSource : DataSourceBase, IConfiguredDataSource
    {
        private readonly Expression _condition;
        private readonly string _originalValueString;

        public ConfiguredDataSource(
            int dataSourceIndex,
            Expression value,
            Expression condition,
            IMemberMappingContext context)
            : base(
                  new ConfiguredQualifiedMember(value, context),
                  GetConvertedValue(dataSourceIndex, value, context),
                  context)
        {
            _originalValueString = GetConvertedValue(dataSourceIndex, value, context).ToString();
            HasConfiguredCondition = condition != null;

            if (HasConfiguredCondition)
            {
                _condition = GetCondition(condition, context);
            }
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

        private static Expression GetCondition(Expression condition, IMemberMappingContext context)
        {
            var conditionNestedAccessesChecks = context
                .NestedAccessFinder
                .FindIn(condition)
                .GetIsNotDefaultComparisonsOrNull();

            if (conditionNestedAccessesChecks == null)
            {
                return condition;
            }

            var checkedCondition = Expression.AndAlso(conditionNestedAccessesChecks, condition);

            return checkedCondition;
        }

        #endregion

        public bool HasConfiguredCondition { get; }

        public override bool IsConditional => base.IsConditional || HasConfiguredCondition;

        public bool IsSameAs(IDataSource otherDataSource)
            => otherDataSource.Value.ToString() == _originalValueString;

        protected override Expression GetValueCondition()
        {
            return HasConfiguredCondition
                ? base.IsConditional ? Expression.AndAlso(base.GetValueCondition(), _condition) : _condition
                : base.GetValueCondition();
        }
    }
}