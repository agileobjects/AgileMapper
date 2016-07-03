namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal abstract class DataSourceBase : IDataSource
    {
        protected DataSourceBase(IQualifiedMember sourceMember, Expression value)
            : this(sourceMember, Enumerable.Empty<Expression>(), Enumerable.Empty<ParameterExpression>(), value)
        {
        }

        protected DataSourceBase(
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

        protected DataSourceBase(
            IQualifiedMember sourceMember,
            Expression value,
            IMemberMappingContext context)
        {
            SourceMember = sourceMember;

            Expression[] nestedAccesses;
            ICollection<ParameterExpression> variables;

            ProcessNestedAccesses(
                context,
                ref value,
                out nestedAccesses,
                out variables);

            NestedAccesses = nestedAccesses;
            Variables = variables;
            Value = value;
        }

        #region Setup

        private static void ProcessNestedAccesses(
            IMemberMappingContext context,
            ref Expression value,
            out Expression[] nestedAccesses,
            out ICollection<ParameterExpression> variables)
        {
            nestedAccesses = context.GetNestedAccessesIn(value);
            variables = new List<ParameterExpression>();

            if (nestedAccesses.None())
            {
                return;
            }

            var nestedAccessVariableByNestedAccess = new Dictionary<Expression, Expression>();

            for (var i = 0; i < nestedAccesses.Length; i++)
            {
                var nestedAccess = nestedAccesses[i];

                if (CacheValueInVariable(nestedAccess))
                {
                    var valueVariable = Expression.Variable(nestedAccess.Type, "accessValue");
                    nestedAccesses[i] = Expression.Assign(valueVariable, nestedAccess);

                    nestedAccessVariableByNestedAccess.Add(nestedAccess, valueVariable);
                    variables.Add(valueVariable);
                }
            }

            value = value.Replace(nestedAccessVariableByNestedAccess);
        }

        private static bool CacheValueInVariable(Expression value)
            => (value.NodeType == ExpressionType.Call) || (value.NodeType == ExpressionType.Invoke);

        #endregion

        public IQualifiedMember SourceMember { get; }

        public virtual bool IsValid => Value != Constants.EmptyExpression;

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