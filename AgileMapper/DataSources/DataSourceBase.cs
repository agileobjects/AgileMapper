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
        private readonly Expression _condition;

        protected DataSourceBase(
            IQualifiedMember sourceMember,
            Expression value,
            IMemberMappingContext context,
            Expression condition = null)
        {
            SourceMember = sourceMember;
            _condition = condition;

            var nestedAccesses = context.NestedAccessFinder.FindIn(value);

            Dictionary<Expression, Expression> nestedAccessVariableByNestedAccess;
            ICollection<ParameterExpression> variables;

            NestedAccesses = ProcessNestedAccesses(
                nestedAccesses,
                out nestedAccessVariableByNestedAccess,
                out variables);

            Variables = variables;

            value = nestedAccessVariableByNestedAccess.Any()
                ? value.Replace(nestedAccessVariableByNestedAccess)
                : value;

            Value = context
                .MapperContext
                .ValueConverters
                .GetConversion(value, context.TargetMember.Type);
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

        public bool IsConditional => _condition != null;

        public IEnumerable<Expression> NestedAccesses { get; }

        public Expression Value { get; }

        public Expression GetIfGuardedPopulation(IMemberMappingContext context)
            => GetGuardedPopulation(context, Expression.IfThen);

        public Expression GetElseGuardedPopulation(Expression populationSoFar, IMemberMappingContext context)
            => GetGuardedPopulation(context, (condition, value) => Expression.IfThenElse(condition, value, populationSoFar));

        private Expression GetGuardedPopulation(
            IMemberMappingContext context,
            Func<Expression, Expression, Expression> guardedPopulationFactory)
        {
            var population = context.TargetMember.GetPopulation(context.InstanceVariable, Value);

            if (!NestedAccesses.Any())
            {
                return IsConditional ? guardedPopulationFactory.Invoke(_condition, population) : population;
            }

            var nestedAccessChecks = NestedAccesses.GetIsNotDefaultComparisons();

            if (!IsConditional)
            {
                return guardedPopulationFactory.Invoke(nestedAccessChecks, population);
            }

            population = Expression.IfThen(nestedAccessChecks, population);

            return guardedPopulationFactory.Invoke(_condition, population);
        }
    }
}