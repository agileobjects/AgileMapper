namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    internal class NullDataSource : IDataSource
    {
        public static readonly IDataSource Instance = new NullDataSource(Constants.EmptyExpression);

        public NullDataSource(Expression value)
        {
            Value = value;
        }

        public IQualifiedMember SourceMember => null;

        public bool IsSuccessful => false;

        public IEnumerable<ParameterExpression> Variables => Enumerable.Empty<ParameterExpression>();

        public Expression GetConditionOrNull(IMemberMappingContext context) => null;

        public IEnumerable<Expression> NestedAccesses => Enumerable.Empty<Expression>();

        public Expression Value { get; }

        public Expression GetIfGuardedPopulation(IMemberMappingContext context)
            => Constants.EmptyExpression;

        public Expression GetElseGuardedPopulation(Expression populationSoFar, IMemberMappingContext context)
            => Constants.EmptyExpression;
    }
}