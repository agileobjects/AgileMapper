namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions;
    using Members.Population;

    internal class StructPopulationExpressionFactory : PopulationExpressionFactoryBase
    {
        public StructPopulationExpressionFactory(ComplexTypeConstructionFactory constructionFactory)
            : base(constructionFactory)
        {
        }

        protected override IEnumerable<Expression> GetPopulationExpressionsFor(
            IMemberPopulation memberPopulation,
            IObjectMappingData mappingData)
        {
            yield return memberPopulation.GetPopulation();
        }

        protected override Expression GetNewObjectCreation(IObjectMappingData mappingData, IList<Expression> memberPopulations)
        {
            var objectCreation = base.GetNewObjectCreation(mappingData, memberPopulations);

            if (memberPopulations.None())
            {
                return objectCreation;
            }

            var memberBindings = GetMemberBindingsFrom(memberPopulations);

            if (memberBindings.None())
            {
                return objectCreation;
            }

            var objectNewings = NewExpressionFinder.FindIn(objectCreation);
            var newingReplacements = new Dictionary<Expression, Expression>(objectNewings.Count);

            foreach (var objectNewing in objectNewings)
            {
                var objectInit = Expression.MemberInit(objectNewing, memberBindings);

                newingReplacements.Add(objectNewing, objectInit);
            }

            var fullObjectInit = objectCreation.Replace(newingReplacements);

            return fullObjectInit;
        }

        private static ICollection<MemberBinding> GetMemberBindingsFrom(IList<Expression> memberPopulations)
        {
            var memberBindings = new List<MemberBinding>(memberPopulations.Count);

            for (var i = memberPopulations.Count - 1; i >= 0; --i)
            {
                var population = memberPopulations[i];

                if (population.NodeType != ExpressionType.Assign)
                {
                    continue;
                }

                var assignment = (BinaryExpression)population;
                var assignedMember = (MemberExpression)assignment.Left;
                var memberBinding = Expression.Bind(assignedMember.Member, assignment.Right);

                memberBindings.Add(memberBinding);
                memberPopulations.RemoveAt(i);
            }

            return memberBindings;
        }

        #region Helper Class

        private class NewExpressionFinder : ExpressionVisitor
        {
            private readonly List<NewExpression> _newings;

            private NewExpressionFinder()
            {
                _newings = new List<NewExpression>();
            }

            public static ICollection<NewExpression> FindIn(Expression objectCreation)
            {
                var finder = new NewExpressionFinder();

                finder.Visit(objectCreation);

                return finder._newings;
            }

            protected override Expression VisitNew(NewExpression newing)
            {
                if (!_newings.Contains(newing))
                {
                    _newings.Add(newing);
                }

                return base.VisitNew(newing);
            }
        }

        #endregion
    }
}