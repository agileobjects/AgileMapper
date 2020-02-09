namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching.Dictionaries;
    using Extensions.Internal;
    using Members;
    using Members.MemberExtensions;
    using ReadableExpressions.Extensions;

    internal abstract class DataSourceBase : IDataSource
    {
        private readonly IList<Expression> _multiInvocations;
        private readonly IList<ParameterExpression> _variables;

        protected DataSourceBase(IQualifiedMember sourceMember, Expression value)
            : this(sourceMember, Enumerable<ParameterExpression>.EmptyArray, value)
        {
        }

        protected DataSourceBase(IDataSource wrappedDataSource, Expression value)
            : this(
                  wrappedDataSource.SourceMember,
                  wrappedDataSource.Variables,
                  value,
                  wrappedDataSource.Condition)
        {
            SourceMemberTypeTest = wrappedDataSource.SourceMemberTypeTest;
        }

        protected DataSourceBase(
            IQualifiedMember sourceMember,
            IList<ParameterExpression> variables,
            Expression value,
            Expression condition = null)
        {
            SourceMember = sourceMember;
            _variables = variables;
            Condition = condition;
            Value = value;
        }

        protected DataSourceBase(
            IQualifiedMember sourceMember,
            Expression value,
            IMemberMapperData mapperData)
        {
            MultiInvocationsHandler.Process(
                value,
                mapperData,
                out _multiInvocations,
                out _variables);

            SourceMember = sourceMember;
            Condition = GetCondition(value, mapperData);
            Value = value;
        }

        #region Setup

        private Expression GetCondition(Expression value, IMemberMapperData mapperData)
        {
            var nestedAccessChecks = mapperData.GetNestedAccessChecksFor(value, targetCanBeNull: false);

            if (nestedAccessChecks == null)
            {
                return null;
            }

            if (IsNotOptionalEntityMemberId(mapperData))
            {
                return nestedAccessChecks;
            }

            var sourceMemberValue = SourceMember.GetQualifiedAccess(mapperData);
            var sourceValueType = sourceMemberValue.Type.GetNonNullableType();

            if (!sourceValueType.IsNumeric())
            {
                return nestedAccessChecks;
            }

            if (sourceMemberValue.Type.IsNullableType())
            {
                sourceMemberValue = sourceMemberValue.GetNullableValueAccess();
            }

            var zero = 0.ToConstantExpression(sourceValueType);
            var sourceValueNonZero = Expression.NotEqual(sourceMemberValue, zero);

            return Expression.AndAlso(nestedAccessChecks, sourceValueNonZero);
        }

        private static bool IsNotOptionalEntityMemberId(IMemberMapperData mapperData)
        {
            var targetMember = mapperData.TargetMember;

            if (!targetMember.Type.IsNullableType())
            {
                return true;
            }

            var targetMemberNameSuffix = default(string);

            for (var i = targetMember.Name.Length - 1; i > 0; --i)
            {
                if (char.IsUpper(targetMember.Name[i]))
                {
                    targetMemberNameSuffix = targetMember.Name.Substring(i).ToLowerInvariant();
                    break;
                }
            }

            switch (targetMemberNameSuffix)
            {
                case "id":
                case "identifier":
                    break;

                default:
                    return true;
            }

            if (!mapperData.TargetTypeIsEntity())
            {
                return true;
            }

            var entityMemberNameLength = targetMember.Name.Length - targetMemberNameSuffix.Length;
            var entityMemberName = targetMember.Name.Substring(0, entityMemberNameLength);

            var entityMember = GlobalContext
                .Instance
                .MemberCache
                .GetTargetMembers(mapperData.TargetType)
                .FirstOrDefault(entityMemberName, (emn, m) => m.Name == emn);

            return !mapperData.IsEntity(entityMember?.Type, out _);
        }

        #endregion

        public IQualifiedMember SourceMember { get; }

        public Expression SourceMemberTypeTest { get; protected set; }

        public virtual bool IsValid => Value != Constants.EmptyExpression;

        public bool IsConditional => Condition != null;

        public virtual bool IsFallback => false;

        public virtual Expression Condition { get; }

        public IList<ParameterExpression> Variables => _variables;

        public Expression Value { get; }

        public virtual Expression AddSourceCondition(Expression value) => value;

        public virtual Expression FinalisePopulationBranch(
            Expression alternatePopulation,
            IMemberMapperData mapperData)
        {
            var population = mapperData.GetTargetMemberPopulation(Value);

            if (IsConditional)
            {
                population = Expression.IfThen(Condition, population);
            }

            switch (_multiInvocations?.Count)
            {
                case null:
                case 0:
                    goto FinalisePopulation;

                case 1:
                    var valueVariable = _variables[0];
                    var valueVariableValue = _multiInvocations[0];
                    var valueVariableAssignment = valueVariable.AssignTo(valueVariableValue);

                    population = population
                        .Replace(
                            valueVariableValue,
                            valueVariable,
                            ExpressionEvaluation.Equivalator)
                        .ReplaceParameter(
                            valueVariable,
                            valueVariableAssignment,
                            replacementCount: 1);

                    goto FinalisePopulation;
            }

            var multiInvocationsCount = _multiInvocations.Count;
            var cachedValuesCount = multiInvocationsCount - 1;

            var valueVariablesByInvocation = FixedSizeExpressionReplacementDictionary
                .WithEquivalentKeys(cachedValuesCount);

            var previousValueVariableAssignment = default(Expression);
            var previousInvocation = default(Expression);
            var previousValueVariable = default(ParameterExpression);

            for (var i = 0; i < multiInvocationsCount; ++i)
            {
                var invocation = _multiInvocations[i];
                var valueVariable = _variables[i];
                var isLastInvocation = i == cachedValuesCount;

                var valueVariableValue = invocation;

                for (int j = 0; j < i; ++j)
                {
                    valueVariableValue = valueVariableValue.Replace(
                        valueVariablesByInvocation.Keys[j],
                        valueVariablesByInvocation.Values[j],
                        ExpressionEvaluation.Equivalator);
                }

                if (!isLastInvocation)
                {
                    valueVariablesByInvocation.Add(valueVariableValue, valueVariable);
                }

                population = population.Replace(
                    valueVariableValue,
                    valueVariable,
                    ExpressionEvaluation.Equivalator);

                Expression valueVariableAssignment;

                if ((previousInvocation != null) && invocation.IsRootedIn(previousInvocation))
                {
                    var chainedValueVariableInvocation = valueVariableValue
                        .Replace(previousValueVariable, previousValueVariableAssignment);

                    valueVariableAssignment = valueVariable.AssignTo(chainedValueVariableInvocation);

                    var chainedAssignmentPopulation = population.Replace(
                        chainedValueVariableInvocation,
                        valueVariableAssignment,
                        ExpressionEvaluation.Equivalator,
                        replacementCount: 1);

                    if (chainedAssignmentPopulation != population)
                    {
                        if (isLastInvocation)
                        {
                            return chainedAssignmentPopulation;
                        }

                        population = chainedAssignmentPopulation;
                        goto SetPreviousValues;
                    }
                }

                valueVariableAssignment = valueVariable.AssignTo(valueVariableValue);

                population = population.ReplaceParameter(
                    valueVariable,
                    valueVariableAssignment,
                    replacementCount: 1);

                SetPreviousValues:
                previousInvocation = invocation;
                previousValueVariable = valueVariable;
                previousValueVariableAssignment = valueVariableAssignment;
            }

            population = Expression.Block(population);

            FinalisePopulation:
            if (alternatePopulation == null)
            {
                return population;
            }

            var ifConditionTruePopulate = (ConditionalExpression)population;

            return ifConditionTruePopulate.Update(
                ifConditionTruePopulate.Test,
                ifConditionTruePopulate.IfTrue,
                alternatePopulation);
        }
    }
}