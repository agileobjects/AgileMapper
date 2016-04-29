namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq.Expressions;
    using Members;

    internal class MemberPopulation
    {
        private static readonly Expression _emptyExpression = Expression.Empty();

        public static MemberPopulation Empty = new MemberPopulation(null, _emptyExpression, _emptyExpression, null);

        public MemberPopulation(
            Member targetMember,
            Expression value,
            Expression population,
            IObjectMappingContext omc)
        {
            TargetMember = targetMember;
            Value = value;
            Population = population;
            ObjectMappingContext = omc;
        }

        public Member TargetMember { get; }

        public Expression Value { get; }

        public Expression Population { get; }

        public IObjectMappingContext ObjectMappingContext { get; }

        public bool IsSuccessful => Population != _emptyExpression;

        public MemberPopulation WithValue(Expression updatedValue)
        {
            return new MemberPopulation(
                TargetMember,
                updatedValue,
                ObjectMappingContext.GetPopulation(TargetMember, updatedValue),
                ObjectMappingContext);
        }

        public MemberPopulation WithPopulation(Expression updatedPopulation)
        {
            return new MemberPopulation(TargetMember, Value, updatedPopulation, ObjectMappingContext);
        }
    }
}