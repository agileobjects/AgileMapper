namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
    using Extensions;
    using Members;

    internal class ValueProvider
    {
        private readonly Expression _value;
        private readonly Expression _condition;
        private readonly Func<IMemberMappingContext, Expression, Expression> _populationFactory;

        private ValueProvider(
            IQualifiedMember sourceMember,
            Expression value,
            IEnumerable<ParameterExpression> variables,
            IEnumerable<Expression> nestedAccesses,
            Expression condition,
            Func<IMemberMappingContext, Expression, Expression> populationFactory)
        {
            SourceMember = sourceMember;
            _value = value;
            Variables = variables;
            NestedAccesses = nestedAccesses;
            _condition = condition;
            IsConditional = _condition != Constants.EmptyExpression;
            _populationFactory = populationFactory;
        }

        #region Factory Methods

        public static ValueProvider For(IDataSource dataSource, IMemberMappingContext context)
        {
            var value = context.Parent
                .MapperContext
                .ValueConverters
                .GetConversion(dataSource.Value, context.TargetMember.Type);

            Dictionary<Expression, Expression> nestedAccessVariableByNestedAccess;
            ICollection<ParameterExpression> variables;

            var nestedAccesses = ProcessNestedAccesses(
                dataSource.NestedAccesses,
                out nestedAccessVariableByNestedAccess,
                out variables);

            value = nestedAccessVariableByNestedAccess.Any()
                ? value.Replace(nestedAccessVariableByNestedAccess)
                : value;

            return new ValueProvider(
                dataSource.SourceMember,
                value,
                variables,
                nestedAccesses,
                null/*dataSource.GetConditionOrNull(context) ?? Constants.EmptyExpression*/,
                GetTargetMemberPopulation);
        }

        private static IEnumerable<Expression> ProcessNestedAccesses(
            IEnumerable<Expression> nestedAccesses,
            out Dictionary<Expression, Expression> nestedAccessVariableByNestedAccess,
            out ICollection<ParameterExpression> variables)
        {
            nestedAccessVariableByNestedAccess = new Dictionary<Expression, Expression>();
            variables = new List<ParameterExpression>();

            var nestedAccessesArray = nestedAccesses
                .OrderBy(access => access.GetDepth())
                .ThenBy(access => access.ToString())
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
        {
            return (value.NodeType == ExpressionType.Call) || (value.NodeType == ExpressionType.Invoke);
        }

        private static Expression GetTargetMemberPopulation(IMemberMappingContext context, Expression finalValue)
            => context.TargetMember.GetPopulation(context.InstanceVariable, finalValue);

        public static ValueProvider Default(IQualifiedMember sourceMember, Type valueType)
            => For(sourceMember, Expression.Default(valueType), GetTargetMemberPopulation);

        public static ValueProvider Null(Func<IMemberMappingContext, Expression> populationFactory)
            => For(NullQualifiedMember.Instance, Constants.EmptyExpression, (context, value) => populationFactory.Invoke(context));

        private static ValueProvider For(IQualifiedMember sourceMember, Expression value, Func<IMemberMappingContext, Expression, Expression> populationFactory)
        {
            return new ValueProvider(
                sourceMember,
                value,
                Enumerable.Empty<ParameterExpression>(),
                Enumerable.Empty<Expression>(),
                Constants.EmptyExpression,
                populationFactory);
        }

        #endregion

        public IQualifiedMember SourceMember { get; }

        public bool IsSuccessful => _value != Constants.EmptyExpression;

        public IEnumerable<ParameterExpression> Variables { get; }

        public IEnumerable<Expression> NestedAccesses { get; }

        public bool IsConditional { get; }

        public Expression GetIfGuardedPopulation(IMemberMappingContext context)
            => GetGuardedPopulation(context, Expression.IfThen);

        public Expression GetElseGuardedPopulation(Expression populationSoFar, IMemberMappingContext context)
            => GetGuardedPopulation(context, (condition, value) => Expression.IfThenElse(condition, value, populationSoFar));

        private Expression GetGuardedPopulation(
            IMemberMappingContext context,
            Func<Expression, Expression, Expression> guardedPopulationFactory)
        {
            var population = _populationFactory.Invoke(context, _value);

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