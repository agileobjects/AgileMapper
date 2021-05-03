namespace AgileObjects.AgileMapper.DataSources
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using Optimisation;

    internal abstract class DataSourceBase : IDataSource
    {
        protected DataSourceBase(IQualifiedMember sourceMember, Expression value)
            : this(sourceMember, Constants.EmptyParameters, value)
        {
        }

        protected DataSourceBase(IDataSource wrappedDataSource, Expression value)
            : this(
                  wrappedDataSource.SourceMember,
                  wrappedDataSource.Variables,
                  value,
                  wrappedDataSource.Condition)
        {
            IsSequential = wrappedDataSource.IsSequential;
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
            Condition = condition;
            Value = value;
        }

        protected DataSourceBase(
            IQualifiedMember sourceMember,
            Expression value,
            IMemberMapperData mapperData)
        {
            SourceMember = sourceMember;
            Variables = Constants.EmptyParameters;
            Condition = GetCondition(value, mapperData);
            Value = value;
        }

        #region Setup

        private Expression GetCondition(Expression value, IMemberMapperData mapperData)
        {
            var nestedAccessChecks = mapperData.GetNestedAccessChecksFor(value);

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

        public bool IsSequential { get; protected set; }

        public virtual bool IsFallback => false;

        public virtual Expression Condition { get; }

        public IList<ParameterExpression> Variables { get; }

        public Expression Value { get; }

        public virtual Expression AddSourceCondition(Expression value) => value;

        public virtual Expression FinalisePopulationBranch(
            Expression alternatePopulation,
            IDataSource nextDataSource,
            IMemberMapperData mapperData)
        {
            MultiInvocationsProcessor.Process(
                this,
                mapperData,
                out var condition,
                out var value,
                out var variables);

            var population = mapperData.GetTargetMemberPopulation(value);

            if (condition != null)
            {
                population = (alternatePopulation == null)
                    ? Expression.IfThen(condition, population)
                    : GetBranchedPopulation(
                        condition,
                        population,
                        alternatePopulation,
                        nextDataSource,
                        mapperData);
            }

            if (variables.Any())
            {
                population = Expression.Block(variables, population);
            }

            return population;
        }

        private static Expression GetBranchedPopulation(
            Expression condition,
            Expression population,
            Expression alternatePopulation,
            IDataSource alternateDataSource,
            IQualifiedMemberContext memberContext)
        {
            if (alternateDataSource.IsSequential && !memberContext.TargetMember.IsSimple)
            {
                return Expression.Block(
                    Expression.IfThen(condition, population),
                    alternatePopulation);
            }

            return Expression.IfThenElse(condition, population, alternatePopulation);
        }
    }
}