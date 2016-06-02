namespace AgileObjects.AgileMapper.DataSources
{
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class ConfiguredDataSource : DataSourceBase, IConfiguredDataSource
    {
        private readonly Expression _condition;
        private readonly string _originalValueString;
        private readonly bool _hasCondition;

        public ConfiguredDataSource(
            int dataSourceIndex,
            Expression value,
            Expression condition,
            IMemberMappingContext context)
            : this(
                  new ConfiguredQualifiedMember(value, context),
                  GetConvertedValue(dataSourceIndex, value, context),
                  condition,
                  context)
        {
        }

        private ConfiguredDataSource(
            IQualifiedMember qualifiedMember,
            Expression convertedValue,
            Expression condition,
            IMemberMappingContext context)
            : base(
                  qualifiedMember,
                  context.WrapInTry(convertedValue),
                  context)
        {
            _originalValueString = convertedValue.ToString();
            _hasCondition = condition != null;

            if (_hasCondition)
            {
                _condition = GetCondition(condition, context);
            }
        }

        #region Setup

        private static Expression GetConvertedValue(int dataSourceIndex, Expression value, IMemberMappingContext context)
        {
            if (context.TargetMember.IsComplex && (context.TargetMember.Type.Assembly != typeof(string).Assembly))
            {
                return ComplexTypeMappingDataSource.GetMapCall(value, dataSourceIndex, context);
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

        public override bool IsConditional => base.IsConditional || _hasCondition;

        public bool IsSameAs(IDataSource otherDataSource)
            => otherDataSource.Value.ToString() == _originalValueString;

        protected override Expression GetValueCondition()
        {
            return _hasCondition
                ? base.IsConditional ? Expression.AndAlso(base.GetValueCondition(), _condition) : _condition
                : base.GetValueCondition();
        }
    }
}