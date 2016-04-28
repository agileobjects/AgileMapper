namespace AgileObjects.AgileMapper.PopulationProcessing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using ObjectPopulation;

    internal class NullNestedSourceMemberPopulationGuarder : IPopulationProcessor
    {
        internal static readonly IPopulationProcessor Instance = new NullNestedSourceMemberPopulationGuarder();

        public IEnumerable<MemberPopulation> Process(IEnumerable<MemberPopulation> populations)
        {
            var guardedPopulations = populations
                .Select(p => new
                {
                    Population = p,
                    AccessedNestedSourceMembers = NestedSourceMemberAccessFinder.Find(p)
                })
                .GroupBy(d => string.Join(",", d.AccessedNestedSourceMembers.Select(m => m.ToString())))
                .OrderBy(grp => grp.Key)
                .SelectMany(grp => (grp.Key == string.Empty)
                    ? grp.Select(d => d.Population)
                    : GroupedAndGuardedPopulationData(
                        grp.First().AccessedNestedSourceMembers,
                        grp.Select(d => d.Population).ToArray()))
                .ToArray();

            return guardedPopulations;
        }

        private static IEnumerable<MemberPopulation> GroupedAndGuardedPopulationData(
            IEnumerable<Expression> accessedNestedSourceMembers,
            IEnumerable<MemberPopulation> populations)
        {
            yield return populations
                .First()
                .ObjectMappingContext.MappingContext.RuleSet
                .NullNestedSourceMemberStrategy
                .Process(accessedNestedSourceMembers, populations);
        }

        #region Helper Class

        private class NestedSourceMemberAccessFinder : ExpressionVisitor
        {
            private readonly Expression _contextSourceParameter;
            private readonly Dictionary<string, Expression> _memberAccessesByPath;

            private NestedSourceMemberAccessFinder(Expression contextSourceParameter)
            {
                _contextSourceParameter = contextSourceParameter;
                _memberAccessesByPath = new Dictionary<string, Expression>();
            }

            public static ICollection<Expression> Find(MemberPopulation population)
            {
                var visitor = new NestedSourceMemberAccessFinder(
                    population.ObjectMappingContext.SourceObject);

                visitor.Visit(population.Value);

                return visitor._memberAccessesByPath.Values;
            }

            protected override Expression VisitMember(MemberExpression memberAccess)
            {
                AddMemberAccessIfAppropriate(memberAccess);

                return base.VisitMember(memberAccess);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCall)
            {
                AddMemberAccessIfAppropriate(methodCall);

                return base.VisitMethodCall(methodCall);
            }

            private void AddMemberAccessIfAppropriate(Expression memberAccess)
            {
                if (Add(memberAccess))
                {
                    _memberAccessesByPath.Add(memberAccess.ToString(), memberAccess);
                }
            }

            private bool Add(Expression memberAccess)
            {
                return (memberAccess.Type != typeof(string)) &&
                       !_memberAccessesByPath.ContainsKey(memberAccess.ToString()) &&
                       memberAccess.Type.CanBeNull() &&
                       memberAccess.IsRootedIn(_contextSourceParameter);
            }
        }

        #endregion
    }
}