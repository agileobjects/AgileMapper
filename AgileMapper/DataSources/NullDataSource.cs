namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    internal class NullDataSource : IDataSource
    {
        public static readonly IDataSource Default = new NullDataSource(Constants.EmptyExpression);

        public NullDataSource(Expression value)
        {
            Value = value;
        }

        public IQualifiedMember SourceMember => null;

        public bool IsValid => false;

        public bool IsConditional => false;

        public IEnumerable<ParameterExpression> Variables => Enumerable.Empty<ParameterExpression>();

        public Expression GetConditionOrNull(IMemberMappingContext context) => null;

        public IEnumerable<Expression> NestedAccesses => Enumerable.Empty<Expression>();

        public Expression Value { get; }

        public Expression GetValueOption(Expression valueSoFar) => Value;
    }
}