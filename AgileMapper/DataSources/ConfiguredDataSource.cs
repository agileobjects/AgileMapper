namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class ConfiguredDataSource : DataSourceBase, IConfiguredDataSource
    {
        private readonly Expression _condition;

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
            OriginalValue = convertedValue;
            _condition = GetCondition(condition, context);
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
            if (condition == null)
            {
                return null;
            }

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

        public bool IsConditional => _condition != null;

        public Expression OriginalValue { get; }

        protected override Expression GetGuardedPopulation(
            IMemberMappingContext context,
            Func<Expression, Expression, Expression> guardedPopulationFactory)
        {
            if (!IsConditional)
            {
                return base.GetGuardedPopulation(context, guardedPopulationFactory);
            }

            var population = context.TargetMember.GetPopulation(context.InstanceVariable, Value);

            if (!NestedAccesses.Any())
            {
                return guardedPopulationFactory.Invoke(_condition, population);
            }

            var nestedAccessChecks = NestedAccesses.GetIsNotDefaultComparisonsOrNull();

            population = Expression.IfThen(nestedAccessChecks, population);

            return guardedPopulationFactory.Invoke(_condition, population);
        }
    }
}