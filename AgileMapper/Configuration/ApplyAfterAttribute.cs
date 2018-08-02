namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    /// <summary>
    /// Marker attribute which indicates the <see cref="MapperConfiguration"/> type to which it is applied
    /// should be applied after one or more <see cref="MapperConfiguration"/> types given in the constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ApplyAfterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyAfterAttribute"/> class.
        /// </summary>
        /// <param name="preceedingMapperConfigurationType">
        /// The <see cref="MapperConfiguration"/> type after which the <see cref="MapperConfiguration"/> type
        /// to which the <see cref="ApplyAfterAttribute"/> is applied, should be applied.
        /// </param>
        public ApplyAfterAttribute(Type preceedingMapperConfigurationType)
        : this(new[] { preceedingMapperConfigurationType })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyAfterAttribute"/> class.
        /// </summary>
        /// <param name="preceedingMapperConfigurationTypes">
        /// The <see cref="MapperConfiguration"/> type(s) after which the <see cref="MapperConfiguration"/> type
        /// to which the <see cref="ApplyAfterAttribute"/> is applied, should be applied.
        /// </param>
        public ApplyAfterAttribute(params Type[] preceedingMapperConfigurationTypes)
        {
            if (preceedingMapperConfigurationTypes.None())
            {
                throw new MappingConfigurationException(
                    $"{nameof(MapperConfiguration)}-derived configuration types must be provided",
                    new ArgumentException("No configuration types provided", nameof(preceedingMapperConfigurationTypes)));
            }

            if (preceedingMapperConfigurationTypes.Any(t => t == null))
            {
                throw new MappingConfigurationException(
                    $"{nameof(MapperConfiguration)}-derived configuration types cannot be null",
                    new ArgumentNullException(nameof(preceedingMapperConfigurationTypes)));
            }

            var nonConfigurationTypes = preceedingMapperConfigurationTypes
                .Filter(t => !t.IsDerivedFrom(typeof(MapperConfiguration)))
                .ToArray();

            if (nonConfigurationTypes.Any())
            {
                var typeNames = string.Join(", ", nonConfigurationTypes.Select(t => t.GetFriendlyName()));

                throw new MappingConfigurationException(
                    $"The following configuration type(s) do not derive from {nameof(MapperConfiguration)}: {typeNames}");
            }

            PreceedingMapperConfigurationTypes = preceedingMapperConfigurationTypes;
        }

        internal Type[] PreceedingMapperConfigurationTypes { get; }
    }
}