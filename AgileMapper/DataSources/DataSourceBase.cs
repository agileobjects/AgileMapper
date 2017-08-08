namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using ReadableExpressions.Extensions;

    internal abstract class DataSourceBase : IDataSource
    {
        protected DataSourceBase(IQualifiedMember sourceMember, Expression value)
            : this(sourceMember, Enumerable<ParameterExpression>.EmptyArray, value)
        {
        }

        protected DataSourceBase(IDataSource wrappedDataSource, Expression value = null)
            : this(
                  wrappedDataSource.SourceMember,
                  wrappedDataSource.Variables,
                  value ?? wrappedDataSource.Value,
                  wrappedDataSource.Condition)
        {
        }

        protected DataSourceBase(
            IQualifiedMember sourceMember,
            ICollection<ParameterExpression> variables,
            Expression value,
            Expression condition = null)
        {
            SourceMember = sourceMember;
            Condition = condition;
            Variables = variables;
            Value = value;
        }

        protected DataSourceBase(
            IQualifiedMember sourceMember,
            Expression value,
            IMemberMapperData mapperData)
        {
            SourceMember = sourceMember;

            ProcessMemberAccesses(
                mapperData,
                ref value,
                out IList<Expression> nestedAccesses,
                out ICollection<ParameterExpression> variables);

            Condition = nestedAccesses.GetIsNotDefaultComparisonsOrNull();
            Variables = variables;
            Value = value;
        }

        #region Setup

        private static void ProcessMemberAccesses(
            IMemberMapperData mapperData,
            ref Expression value,
            out IList<Expression> nestedAccesses,
            out ICollection<ParameterExpression> variables)
        {
            var valueInfo = mapperData.GetExpressionInfoFor(value, targetCanBeNull: false);
            nestedAccesses = valueInfo.NestedAccesses;

            if (valueInfo.MultiInvocations.None())
            {
                variables = Enumerable<ParameterExpression>.EmptyArray;
                return;
            }

            variables = new List<ParameterExpression>();
            var cacheVariablesByValue = new Dictionary<Expression, Expression>();
            var valueExpressions = new List<Expression>(valueInfo.MultiInvocations.Count + 1);

            foreach (var invocation in valueInfo.MultiInvocations)
            {
                var valueVariableName = invocation.Type.GetFriendlyName().ToCamelCase() + "Value";
                var valueVariable = Expression.Variable(invocation.Type, valueVariableName);
                var valueVariableValue = invocation.Replace(cacheVariablesByValue);

                cacheVariablesByValue.Add(invocation, valueVariable);
                variables.Add(valueVariable);
                valueExpressions.Add(valueVariable.AssignTo(valueVariableValue));
            }

            valueExpressions.Add(value.Replace(cacheVariablesByValue));
            value = Expression.Block(valueExpressions);
        }

        #endregion

        public IQualifiedMember SourceMember { get; }

        public Expression SourceMemberTypeTest { get; protected set; }

        public virtual bool IsValid => Value != Constants.EmptyExpression;

        public virtual Expression PreCondition => null;

        public bool IsConditional => Condition != null;

        public virtual Expression Condition { get; }

        public ICollection<ParameterExpression> Variables { get; }

        public Expression Value { get; }

        public virtual Expression AddPreCondition(Expression population) => population;

        public Expression AddCondition(Expression value, Expression alternateBranch = null)
        {
            return alternateBranch != null
                ? Expression.IfThenElse(Condition, value, alternateBranch)
                : Expression.IfThen(Condition, value);
        }

        public Expression GetTargetMemberPopulation(IMemberMapperData mapperData)
            => mapperData.GetTargetMemberPopulation(Value);
    }
}