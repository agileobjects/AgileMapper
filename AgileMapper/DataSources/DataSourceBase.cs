namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal abstract class DataSourceBase : IDataSource
    {
        private readonly ICollection<IObjectMapper> _inlineObjectMappers;

        protected DataSourceBase(IQualifiedMember sourceMember, MapCall mapCall)
            : this(sourceMember, mapCall.Call)
        {
            if (mapCall.InlineMapper != null)
            {
                _inlineObjectMappers.Add(mapCall.InlineMapper);
            }
        }

        protected DataSourceBase(IQualifiedMember sourceMember, Expression value)
            : this(sourceMember, Enumerable.Empty<ParameterExpression>(), value)
        {
        }

        protected DataSourceBase(
            IQualifiedMember sourceMember,
            IEnumerable<ParameterExpression> variables,
            Expression value,
            Expression condition = null)
            : this(sourceMember)
        {
            Condition = condition;
            Variables = variables;
            Value = value;
        }

        protected DataSourceBase(
            IQualifiedMember sourceMember,
            Expression value,
            MemberMapperData data)
            : this(sourceMember)
        {
            Expression[] nestedAccesses;
            ICollection<ParameterExpression> variables;

            ProcessNestedAccesses(
                data,
                ref value,
                out nestedAccesses,
                out variables);

            Condition = nestedAccesses.GetIsNotDefaultComparisonsOrNull();
            Variables = variables;
            Value = value;
        }

        private DataSourceBase(IQualifiedMember sourceMember)
        {
            SourceMember = sourceMember;
            _inlineObjectMappers = new List<IObjectMapper>();
        }

        #region Setup

        private static void ProcessNestedAccesses(
            MemberMapperData data,
            ref Expression value,
            out Expression[] nestedAccesses,
            out ICollection<ParameterExpression> variables)
        {
            nestedAccesses = data.GetNestedAccessesIn(value);
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

        public bool IsConditional => Condition != null;

        public virtual Expression Condition { get; }

        public IEnumerable<ParameterExpression> Variables { get; }

        public IEnumerable<IObjectMapper> InlineObjectMappers => _inlineObjectMappers;

        public Expression Value { get; }

        public virtual Expression GetValueOption(Expression valueSoFar)
            => Expression.Condition(Condition, Value, valueSoFar);
    }
}