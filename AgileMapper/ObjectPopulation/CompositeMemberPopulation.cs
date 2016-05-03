namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal class CompositeMemberPopulation : IMemberPopulation
    {
        private readonly IEnumerable<IMemberPopulation> _memberPopulations;
        private readonly IMemberPopulation _examplePopulation;
        private readonly ICollection<Expression> _conditions;

        public CompositeMemberPopulation(IEnumerable<IMemberPopulation> memberPopulations)
        {
            _memberPopulations = memberPopulations;
            _examplePopulation = memberPopulations.First();
            _conditions = new List<Expression>();
        }

        public IObjectMappingContext ObjectMappingContext => _examplePopulation.ObjectMappingContext;

        public Member TargetMember => _examplePopulation.TargetMember;

        public IEnumerable<Expression> NestedSourceMemberAccesses => _examplePopulation.NestedSourceMemberAccesses;

        public bool IsMultiplePopulation => _memberPopulations.Count() > 1;

        public Expression Value => _examplePopulation.Value;

        public bool IsSuccessful => _memberPopulations.Any(p => p.IsSuccessful);

        public IMemberPopulation AddCondition(Expression condition)
        {
            _conditions.Add(condition);
            return this;
        }

        public IMemberPopulation WithValue(Expression updatedValue)
        {
            return _examplePopulation.WithValue(updatedValue);
        }

        public Expression GetPopulation()
        {
            Expression population = Expression.Block(_memberPopulations.Select(p => p.GetPopulation()));

            if (_conditions.Any())
            {
                var allConditions = _conditions.GetIsNotDefaultComparisons();

                population = Expression.IfThen(allConditions, population);
            }

            return population;
        }
    }
}