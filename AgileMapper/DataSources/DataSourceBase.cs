namespace AgileObjects.AgileMapper.DataSources
{
    using System;
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

            var nestedAccessesArray = nestedAccesses
            //    .OrderBy(access => access.GetDepth())
            //    .ThenBy(access => access.ToString())
                .ToArray();

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

        public bool IsSuccessful => Value != Constants.EmptyExpression;

        public IEnumerable<ParameterExpression> Variables { get; }

        public IEnumerable<Expression> NestedAccesses { get; }

        public bool HasValue(Expression value) => false;

        public Expression Value { get; }

        public Expression GetIfGuardedPopulation(IMemberMappingContext context)
            => GetGuardedPopulation(context, Expression.IfThen);

        public Expression GetElseGuardedPopulation(Expression populationSoFar, IMemberMappingContext context)
            => GetGuardedPopulation(context, (condition, value) => Expression.IfThenElse(condition, value, populationSoFar));

        protected virtual Expression GetGuardedPopulation(
            IMemberMappingContext context,
            Func<Expression, Expression, Expression> guardedPopulationFactory)
        {
            var population = context.TargetMember.GetPopulation(context.InstanceVariable, Value);

            if (NestedAccesses.None())
            {
                return population;
            }

            var nestedAccessChecks = NestedAccesses.GetIsNotDefaultComparisonsOrNull();

            return guardedPopulationFactory.Invoke(nestedAccessChecks, population);
        }
    }
}