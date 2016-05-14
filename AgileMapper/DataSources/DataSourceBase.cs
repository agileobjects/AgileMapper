namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;
    using ObjectPopulation;

    internal abstract class DataSourceBase : IDataSource
    {
        private readonly Func<IMemberMappingContext, Expression> _conditionFactory;

        protected DataSourceBase(
            Expression value,
            IMemberMappingContext context,
            Func<IMemberMappingContext, Expression> conditionFactory = null)
            : this(value, context.NestedAccessFinder.FindIn(value), conditionFactory)
        {
        }

        protected DataSourceBase(Expression value)
           : this(value, Enumerable.Empty<Expression>())
        {
        }

        protected DataSourceBase(
            Expression value,
            IEnumerable<Expression> nestedAccesses,
            Func<IMemberMappingContext, Expression> conditionFactory = null)
        {
            _conditionFactory = conditionFactory;
            NestedAccesses = nestedAccesses;
            Value = value;
        }

        public IEnumerable<ValueProvider> GetValueProviders(IMemberMappingContext context)
            => GetValueProvidersEnumerable(context).ToArray();

        private IEnumerable<ValueProvider> GetValueProvidersEnumerable(IMemberMappingContext context)
        {
            var primaryValueProvider = ValueProvider.For(this, context);

            yield return primaryValueProvider;

            if (primaryValueProvider.IsConditional || NestedAccesses.Any())
            {
                var alternateValueProviders = GetAlternateValueProvidersOrNull(context);

                if (alternateValueProviders != null)
                {
                    foreach (var alternateValueProvider in alternateValueProviders)
                    {
                        yield return alternateValueProvider;
                    }
                }
            }
        }

        protected virtual IEnumerable<ValueProvider> GetAlternateValueProvidersOrNull(IMemberMappingContext context)
        {
            yield return context.Parent.MappingContext.RuleSet.FallbackValueProviderFactory.Create(context);
        }

        public Expression GetConditionOrNull(IMemberMappingContext context)
            => _conditionFactory?.Invoke(context);

        public IEnumerable<Expression> NestedAccesses { get; }

        public Expression Value { get; }
    }
}