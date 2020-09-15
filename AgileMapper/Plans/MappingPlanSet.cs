namespace AgileObjects.AgileMapper.Plans
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Extensions;
    using Extensions.Internal;
    using static System.Environment;
#if NET35
    using Expr = Microsoft.Scripting.Ast.Expression;
#else
    using Expr = System.Linq.Expressions.Expression;
#endif

    /// <summary>
    /// Contains sets of details of mapping plans for mappings between a particular source and target types,
    /// for particular mapping types (create new, merge, overwrite).
    /// </summary>
    public class MappingPlanSet : IEnumerable<IMappingPlan>
    {
        private readonly IEnumerable<IMappingPlan> _mappingPlans;

        internal MappingPlanSet(IEnumerable<IMappingPlan> mappingPlans)
        {
            _mappingPlans = mappingPlans;
        }

        internal static MappingPlanSet For(MapperContext mapperContext)
        {
            return new MappingPlanSet(mapperContext
                .ObjectMapperFactory
                .RootMappers
                .Project(mapper => new MappingPlan(mapper))
                .ToArray());
        }

        /// <summary>
        /// Converts the given <paramref name="mappingPlans">MappingPlanSet</paramref> to its string 
        /// representation.
        /// </summary>
        /// <param name="mappingPlans">The <see cref="MappingPlanSet"/> to convert.</param>
        /// <returns>
        /// The string representation of the <paramref name="mappingPlans">MappingPlanSet</paramref>.
        /// </returns>
        public static implicit operator string(MappingPlanSet mappingPlans)
        {
            return mappingPlans
                .Project(plan => plan.ToSourceCode())
                .Join(NewLine + NewLine);
        }

        /// <summary>
        /// Converts the given <paramref name="mappingPlans">MappingPlanSet</paramref> to a collection
        /// of Expressions.
        /// </summary>
        /// <param name="mappingPlans">The <see cref="MappingPlanSet"/> to convert.</param>
        /// <returns>
        /// A collection of Expressions representing this <paramref name="mappingPlans">MappingPlanSet</paramref>.
        /// </returns>
        public static implicit operator ReadOnlyCollection<Expr>(MappingPlanSet mappingPlans)
        {
            return new ReadOnlyCollection<Expr>(mappingPlans
                .Select(mp =>
                {
                    var functionBlocks = mp
                        .Select(mpf => (Expr)Expr.Block(mpf.Summary, mpf.Mapping))
                        .ToList();

                    return functionBlocks.HasOne()
                        ? functionBlocks.First()
                        : Expr.Block(functionBlocks);
                })
                .ToList());
        }

        IEnumerator IEnumerable.GetEnumerator() => _mappingPlans.GetEnumerator();

        IEnumerator<IMappingPlan> IEnumerable<IMappingPlan>.GetEnumerator()
            => _mappingPlans.GetEnumerator();

        /// <summary>
        /// Returns the string representation of the <see cref="MappingPlanSet"/>.
        /// </summary>
        /// <returns>The string representation of the <see cref="MappingPlanSet"/>.</returns>
        public override string ToString() => this;
    }
}