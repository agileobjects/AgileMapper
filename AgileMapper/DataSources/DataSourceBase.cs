namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal abstract class DataSourceBase : IDataSource
    {
        protected DataSourceBase(
            IQualifiedMember sourceMember,
            Expression value,
            IMemberMappingContext context)
        {
            SourceMember = sourceMember;

            var valueNestedAccesses = context.NestedAccessFinder.FindIn(value);

            Dictionary<Expression, Expression> nestedAccessVariableByNestedAccess;
            ICollection<ParameterExpression> variables;

            NestedAccesses = ProcessNestedAccesses(
                valueNestedAccesses,
                out nestedAccessVariableByNestedAccess,
                out variables);

            Variables = variables;

            Value = nestedAccessVariableByNestedAccess.Any()
                ? value.Replace(nestedAccessVariableByNestedAccess)
                : value;
        }

        protected DataSourceBase(IQualifiedMember sourceMember, Expression value)
            : this(sourceMember, Enumerable.Empty<Expression>(), Enumerable.Empty<ParameterExpression>(), value)
        {
        }

        protected DataSourceBase(IDataSource wrappedDataSource, Expression value)
            : this(
                  wrappedDataSource.SourceMember,
                  wrappedDataSource.NestedAccesses,
                  wrappedDataSource.Variables,
                  value)
        {
        }

        private DataSourceBase(
            IQualifiedMember sourceMember,
            IEnumerable<Expression> nestedAccesses,
            IEnumerable<ParameterExpression> variables,
            Expression value)
        {
            SourceMember = sourceMember;
            NestedAccesses = nestedAccesses;
            Variables = variables;
            Value = value;
        }

        #region Setup

        private static IEnumerable<Expression> ProcessNestedAccesses(
            IEnumerable<Expression> nestedAccesses,
            out Dictionary<Expression, Expression> nestedAccessVariableByNestedAccess,
            out ICollection<ParameterExpression> variables)
        {
            nestedAccessVariableByNestedAccess = new Dictionary<Expression, Expression>();
            variables = new List<ParameterExpression>();

            var nestedAccessesArray = nestedAccesses.ToArray();

            for (var i = 0; i < nestedAccessesArray.Length; i++)
            {
                var nestedAccess = nestedAccessesArray[i];

                if (CacheValueInVariable(nestedAccess))
                {
                    var valueVariable = Expression.Variable(nestedAccess.Type, "accessValue");
                    nestedAccessesArray[i] = Expression.Assign(valueVariable, nestedAccess);

                    nestedAccessVariableByNestedAccess.Add(nestedAccess, valueVariable);
                    variables.Add(valueVariable);
                }
            }

            return nestedAccessesArray;
        }

        private static bool CacheValueInVariable(Expression value)
            => (value.NodeType == ExpressionType.Call) || (value.NodeType == ExpressionType.Invoke);

        #endregion

        public IQualifiedMember SourceMember { get; }

        public bool IsValid => Value != Constants.EmptyExpression;

        public virtual bool IsConditional => NestedAccesses.Any();

        public IEnumerable<ParameterExpression> Variables { get; }

        public IEnumerable<Expression> NestedAccesses { get; }

        public Expression Value { get; }

        public virtual Expression GetValueOption(Expression valueSoFar)
            => Expression.Condition(GetValueCondition(), Value, valueSoFar);

        protected virtual Expression GetValueCondition()
            => NestedAccesses.GetIsNotDefaultComparisonsOrNull();
    }
}