namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataSources;
    using Members;
    using ReadableExpressions;

    internal class MemberPopulation
    {
        public MemberPopulation(
            Member targetMember,
            IDataSource dataSource,
            IObjectMappingContext omc)
            : this(
                  targetMember,
                  omc.MapperContext.ValueConverters.GetConversion(dataSource.Value, targetMember.Type),
                  dataSource.NestedSourceMemberAccesses,
                  omc)
        {
        }

        private MemberPopulation(
            Member targetMember,
            Expression value,
            IEnumerable<Expression> nestedSourceMemberAccesses,
            IObjectMappingContext omc)
            : this(
                  targetMember,
                  value,
                  nestedSourceMemberAccesses,
                  targetMember.GetPopulation(omc.TargetVariable, value), omc)
        {
        }

        private MemberPopulation(
            Member targetMember,
            Expression value,
            IEnumerable<Expression> nestedSourceMemberAccesses,
            Expression population,
            IObjectMappingContext omc)
        {
            TargetMember = targetMember;
            Value = value;
            NestedSourceMemberAccesses = nestedSourceMemberAccesses;
            Population = population;
            ObjectMappingContext = omc;
        }

        #region Factory Methods

        public static MemberPopulation IgnoredMember(Member targetMember, IObjectMappingContext omc)
            => new MemberPopulation(
                   targetMember,
                   Constants.EmptyExpression,
                   Enumerable.Empty<Expression>(),
                   ReadableExpression.Comment(targetMember.Name + " is ignored"),
                   omc);

        public static MemberPopulation NoDataSource(Member targetMember, IObjectMappingContext omc)
            => new MemberPopulation(
                   targetMember,
                   Constants.EmptyExpression,
                   Enumerable.Empty<Expression>(),
                   ReadableExpression.Comment("No data source for " + targetMember.Name),
                   omc);

        #endregion

        public Member TargetMember { get; }

        public Expression Value { get; }

        public IEnumerable<Expression> NestedSourceMemberAccesses { get; }

        public Expression Population { get; }

        public bool IsSuccessful => Population != null;

        public IObjectMappingContext ObjectMappingContext { get; }

        public MemberPopulation WithValue(Expression updatedValue)
            => new MemberPopulation(TargetMember, updatedValue, NestedSourceMemberAccesses, ObjectMappingContext);

        public MemberPopulation WithPopulation(Expression updatedPopulation)
            => new MemberPopulation(TargetMember, Value, NestedSourceMemberAccesses, updatedPopulation, ObjectMappingContext);
    }
}