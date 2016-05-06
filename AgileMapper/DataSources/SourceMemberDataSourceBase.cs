namespace AgileObjects.AgileMapper.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Members;

    internal abstract class SourceMemberDataSourceBase : IDataSource
    {
        private readonly Func<IMemberMappingContext, Expression> _conditionFactory;

        protected SourceMemberDataSourceBase(
            Expression value,
            IMemberMappingContext context,
            Func<IMemberMappingContext, Expression> conditionFactory = null)
        {
            _conditionFactory = conditionFactory;
            NestedSourceMemberAccesses = context.NestedAccessFinder.FindIn(value);
            Value = value;
        }

        public Expression GetConditionOrNull(IMemberMappingContext context)
        {
            return _conditionFactory?.Invoke(context);
        }

        public IEnumerable<Expression> NestedSourceMemberAccesses { get; }

        public Expression Value { get; }
    }
}