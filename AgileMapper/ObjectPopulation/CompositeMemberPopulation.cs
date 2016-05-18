namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    internal class CompositeMemberPopulation : IMemberPopulation
    {
        private readonly IEnumerable<IMemberPopulation> _memberPopulations;
        private readonly IMemberPopulation _examplePopulation;
        private Expression _condition;

        public CompositeMemberPopulation(IEnumerable<IMemberPopulation> memberPopulations)
        {
            _memberPopulations = memberPopulations;
            _examplePopulation = memberPopulations.First();
        }

        public IObjectMappingContext ObjectMappingContext => _examplePopulation.ObjectMappingContext;

        public IQualifiedMember TargetMember => _examplePopulation.TargetMember;

        public IEnumerable<Expression> NestedAccesses => _examplePopulation.NestedAccesses;

        public bool IsSuccessful => _memberPopulations.Any(p => p.IsSuccessful);

        public IMemberPopulation WithCondition(Expression condition)
        {
            _condition = condition;
            return this;
        }

        public Expression GetPopulation()
        {
            Expression population = Expression.Block(_memberPopulations.Select(p => p.GetPopulation()));

            if (_condition != null)
            {
                population = Expression.IfThen(_condition, population);
            }

            return population;
        }
    }
}