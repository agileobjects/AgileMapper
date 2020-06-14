namespace AgileObjects.AgileMapper.ObjectPopulation.ComplexTypes
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Caching.Dictionaries;
    using Extensions.Internal;
    using Members.Population;

    internal class MemberInitPopulationExpressionFactory : PopulationExpressionFactoryBase
    {
        protected override IEnumerable<Expression> GetPopulationExpressionsFor(
            IMemberPopulator memberPopulator,
            IObjectMappingData mappingData)
        {
            yield return memberPopulator.GetPopulation();
        }

        protected override Expression GetTargetObjectCreation(
            IObjectMappingData mappingData, 
            IList<Expression> memberPopulations)
        {
            var objectCreation = base.GetTargetObjectCreation(mappingData, memberPopulations);

            if (objectCreation == null)
            {
                memberPopulations.Clear();
                return null;
            }

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
            var newingReplacements = FixedSizeExpressionReplacementDictionary.WithEqualKeys(objectNewings.Count);

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
            var memberPopulationCount = memberPopulations.Count;
            var memberBindings = new MemberBinding[memberPopulationCount];

            for (var i = memberPopulationCount - 1; i >= 0; --i)
            {
                var population = memberPopulations[i];

                if (population.NodeType != ExpressionType.Assign)
                {
                    continue;
                }

                var assignment = (BinaryExpression)population;
                var assignedMember = (MemberExpression)assignment.Left;
                var memberBinding = Expression.Bind(assignedMember.Member, assignment.Right);

                memberBindings[i] = memberBinding;
                memberPopulations.RemoveAt(i);
            }

            var addedBindingsCount = memberPopulationCount - memberPopulations.Count;

            if (addedBindingsCount == 0)
            {
                return Enumerable<MemberBinding>.EmptyArray;
            }

            if (addedBindingsCount == memberPopulationCount)
            {
                return memberBindings;
            }

            var nonNullBindings = new MemberBinding[addedBindingsCount];
            var bindingIndex = 0;

            while (true)
            {
                nonNullBindings[bindingIndex++] = memberBindings[--memberPopulationCount];

                if (bindingIndex == addedBindingsCount)
                {
                    break;
                }
            }

            return nonNullBindings;
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