namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching.Dictionaries;
    using Extensions.Internal;
    using Members;
    using ReadableExpressions.Extensions;

    internal abstract class DataSourceBase : IDataSource
    {
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
            Variables = variables;
            Value = value;
            Condition = condition;
        }

        protected DataSourceBase(
            IQualifiedMember sourceMember,
            Expression value,
            IMemberMapperData mapperData)
        {
            var nestedAccessChecks = mapperData.GetNestedAccessChecksFor(value, targetCanBeNull: false);

            SourceMember = sourceMember;
            Condition = GetCondition(nestedAccessChecks, mapperData);
            Value = HandleMultiInvocations(value, out var variables, mapperData);
            Variables = variables;
        }

        #region Setup

        private Expression HandleMultiInvocations(
            Expression value,
            out IList<ParameterExpression> variables,
            IMemberMapperData mapperData)
        {
            var multiInvocations = mapperData.GetMultiInvocationsFor(value);

            if (multiInvocations.None())
            {
                variables = Enumerable<ParameterExpression>.EmptyArray;
                return value;
            }

            // TODO: Optimise for single multi-invocation
            var multiInvocationsCount = multiInvocations.Count;
            variables = new ParameterExpression[multiInvocationsCount];
            var cacheVariablesByValue = FixedSizeExpressionReplacementDictionary.WithEqualKeys(multiInvocationsCount);
            var valueExpressions = new Expression[multiInvocationsCount + 1];

            for (var i = 0; i < multiInvocationsCount; ++i)
            {
                var invocation = multiInvocations[i];
                var valueVariableName = invocation.Type.GetVariableNameInCamelCase() + "Value";
                var valueVariable = Expression.Variable(invocation.Type, valueVariableName);
                var valueVariableValue = invocation.Replace(cacheVariablesByValue);

                cacheVariablesByValue.Add(invocation, valueVariable);
                variables[i] = valueVariable;
                valueExpressions[i] = valueVariable.AssignTo(valueVariableValue);
            }

            valueExpressions[multiInvocationsCount] = value.Replace(cacheVariablesByValue);

            return Expression.Block(valueExpressions);
        }

        private Expression GetCondition(Expression nestedAccessChecks, IMemberMapperData mapperData)
        {
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

        public IList<ParameterExpression> Variables { get; }

        public Expression Value { get; }

        public virtual Expression AddSourceCondition(Expression value) => value;

        public virtual Expression FinalisePopulation(Expression population, Expression alternatePopulation)
        {
            if (IsConditional)
            {
                population = (alternatePopulation != null)
                    ? Expression.IfThenElse(Condition, population, alternatePopulation)
                    : Expression.IfThen(Condition, population);
            }

            return population;
        }
    }
}