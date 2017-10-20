namespace AgileObjects.AgileMapper.Plans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ObjectPopulation;

    internal class CachedMapperMappingPlan : IMappingPlan
    {
        private readonly List<IObjectMapper> _cachedMappers;

        public CachedMapperMappingPlan(IObjectMapper mapper)
        {
            _cachedMappers = new List<IObjectMapper>();

            Expand(mapper);
        }

        private void Expand(IObjectMapper mapper)
        {
            _cachedMappers.Add(mapper);

            foreach (var subMapper in mapper.SubMappers)
            {
                Expand(subMapper);
            }
        }

        /// <summary>
        /// Converts the given <paramref name="mappingPlan"/> to its string representation.
        /// </summary>
        /// <param name="mappingPlan">The <see cref="CachedMapperMappingPlan"/> to convert.</param>
        /// <returns>The string representation of the given <paramref name="mappingPlan"/>.</returns>
        public static implicit operator string(CachedMapperMappingPlan mappingPlan)
        {
            return string.Join(
                Environment.NewLine + Environment.NewLine,
                mappingPlan._cachedMappers.Select(GetDescription));
        }

        private static string GetDescription(IObjectMapper mapper)
            => MappingPlanFunction.For(mapper.MappingLambda, mapper.MapperData);

        public override string ToString() => this;
    }
}