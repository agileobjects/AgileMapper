namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal class ConfiguredDataSource : DataSourceBase
    {
        public ConfiguredDataSource(
            Expression value,
            IMemberMappingContext context,
            Func<IMemberMappingContext, Expression> conditionFactory)
            : base(value, context, conditionFactory)
        {
        }

        protected override IEnumerable<ValueProvider> GetAlternateValueProvidersOrNull(IMemberMappingContext context)
        {
            var alternateDataSource = context.Parent
                .MapperContext
                .DataSources
                .FindFor(context, DataSourceOption.ExcludeConfigured);

            if (alternateDataSource == null)
            {
                return null;
            }

            return ValuesAreTheSame(alternateDataSource)
                ? base.GetAlternateValueProvidersOrNull(context)
                : alternateDataSource.GetValueProviders(context);
        }

        private bool ValuesAreTheSame(IDataSource otherDataSource)
            => otherDataSource.Value.ToString() == Value.ToString();
    }
}