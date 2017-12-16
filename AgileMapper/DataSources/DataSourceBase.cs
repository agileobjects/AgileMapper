namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions.Internal;
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
                out var nestedAccesses,
                out var variables);

            Condition = nestedAccesses.GetIsNotDefaultComparisonsOrNull();
            Variables = variables;
            Value = value;
        }

        #region Setup

        private static void ProcessMemberAccesses(
            IMemberMapperData mapperData,
            ref Expression value,
            out IList<Expression> nestedAccesses,
            out IList<ParameterExpression> variables)
        {
            var valueInfo = mapperData.GetExpressionInfoFor(value, targetCanBeNull: false);
            nestedAccesses = valueInfo.NestedAccesses;

            if (valueInfo.MultiInvocations.None())
            {
                variables = Enumerable<ParameterExpression>.EmptyArray;
                return;
            }

            var numberOfInvocations = valueInfo.MultiInvocations.Count;
            variables = new ParameterExpression[numberOfInvocations];
            var cacheVariablesByValue = new Dictionary<Expression, Expression>(numberOfInvocations);
            var valueExpressions = new Expression[numberOfInvocations + 1];

            for (var i = 0; i < numberOfInvocations; i++)
            {
                var invocation = valueInfo.MultiInvocations[i];
                var valueVariableName = invocation.Type.GetFriendlyName().ToCamelCase() + "Value";
                var valueVariable = Expression.Variable(invocation.Type, valueVariableName);
                var valueVariableValue = invocation.Replace(cacheVariablesByValue);

                cacheVariablesByValue.Add(invocation, valueVariable);
                variables[i] = valueVariable;
                valueExpressions[i] = valueVariable.AssignTo(valueVariableValue);
            }

            valueExpressions[numberOfInvocations] = value.Replace(cacheVariablesByValue);
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