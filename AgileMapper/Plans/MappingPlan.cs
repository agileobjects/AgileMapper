namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
            _mappingPlanFunctions = new List<IMappingPlanFunction>
            {
                new RootMapperMappingPlanFunction(cachedMapper)
            };

            if (cachedMapper.MapperData.HasMapperFuncs)
            {
                _mappingPlanFunctions.AddRange(cachedMapper
                    .RecursionMapperFuncs
                    .Select(mf => new RecursionMapperMappingPlanFunction(mf)));
            }
        }

        internal static MappingPlan For<TSource, TTarget>(IMappingContext mappingContext)
        {
            var mappingData = ObjectMappingDataFactory
                .ForRootFixedTypes(default(TSource), default(TTarget), mappingContext);

            return new MappingPlan(mappingData.Mapper);
        }

        /// <summary>
        /// Converts the given <paramref name="mappingPlan"/> to its string representation.
        /// </summary>
        /// <param name="mappingPlan">The <see cref="MappingPlan"/> to convert.</param>
        /// <returns>The string representation of the given <paramref name="mappingPlan"/>.</returns>
        public static implicit operator string(MappingPlan mappingPlan)
        {
            return string.Join(
                Environment.NewLine + Environment.NewLine,
                mappingPlan._mappingPlanFunctions.Select(pd => pd.GetDescription()));
        }

        /// <summary>
        /// Returns the string representation of the <see cref="MappingPlan"/>.
        /// </summary>
        /// <returns>The string representation of the <see cref="MappingPlan"/>.</returns>
        public override string ToString() => this;
    }
}
