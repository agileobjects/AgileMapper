namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Extensions.Internal;
    using ObjectPopulation;

    /// <summary>
    /// Contains details of the mapping plan for a mapping between a particular source and target type,
    /// for a particular mapping type (create new, merge, overwrite).
    /// </summary>
    public class MappingPlan : IMappingPlan
    {
        private readonly List<IMappingPlanFunction> _mappingPlanFunctions;

        internal MappingPlan(IObjectMapper cachedMapper)
        {
            Root = new RootMapperMappingPlanFunction(cachedMapper);

            _mappingPlanFunctions = new List<IMappingPlanFunction> { Root };

            if (cachedMapper.MapperData.HasRepeatedMapperFuncs)
            {
                _mappingPlanFunctions.AddRange(cachedMapper
                    .RepeatedMappingFuncs
                    .Project(mf => new RepeatedMappingMappingPlanFunction(mf)));
            }
        }

        internal static MappingPlan For(IObjectMappingData mappingData)
            => new MappingPlan(mappingData.GetOrCreateMapper());

        /// <summary>
        /// Converts the given <paramref name="mappingPlan"/> to its string representation.
        /// </summary>
        /// <param name="mappingPlan">The <see cref="MappingPlan"/> to convert.</param>
        /// <returns>The string representation of the given <paramref name="mappingPlan"/>.</returns>
        public static implicit operator string(MappingPlan mappingPlan)
        {
            return mappingPlan
                ._mappingPlanFunctions
                .ProjectToArray(pf => pf.ToSourceCode())
                .Join(Environment.NewLine + Environment.NewLine);
        }

        /// <summary>
        /// Converts the given <paramref name="mappingPlan"/> to an Expression.
        /// </summary>
        /// <param name="mappingPlan">The <see cref="MappingPlan"/> to convert.</param>
        /// <returns>An Expression representation of the given <paramref name="mappingPlan"/>.</returns>
        public static implicit operator Expression(MappingPlan mappingPlan)
        {
            return Expression.Block(mappingPlan
                ._mappingPlanFunctions
                .SelectMany(mpf => new Expression[] { mpf.Summary, mpf.Mapping }));
        }

        /// <inheritdoc />
        public IMappingPlanFunction Root { get; }

        string IMappingPlan.ToSourceCode() => this;

        IEnumerator IEnumerable.GetEnumerator() => _mappingPlanFunctions.GetEnumerator();

        IEnumerator<IMappingPlanFunction> IEnumerable<IMappingPlanFunction>.GetEnumerator()
            => _mappingPlanFunctions.GetEnumerator();

        /// <summary>
        /// Returns the string representation of the <see cref="MappingPlan"/>.
        /// </summary>
        /// <returns>The string representation of the <see cref="MappingPlan"/>.</returns>
        public override string ToString() => this;
    }
}
