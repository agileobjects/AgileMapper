namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using AgileMapper.Configuration;
    using Extensions.Internal;
    using NetStandardPolyfills;

    /// <summary>
    /// Provides options for specifying <see cref="MapperConfiguration"/> instances with which to
    /// perform Mapper configuration.
    /// </summary>
    public class MapperConfigurationSpecifier
    {
        private readonly IMapper _mapper;

        internal MapperConfigurationSpecifier(IMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Apply the configuration in the <see cref="MapperConfiguration"/> of the given
        /// <typeparamref name="TConfiguration"/> instance.
        /// </summary>
        /// <typeparam name="TConfiguration">The <see cref="MapperConfiguration"/> implementation to apply.</typeparam>
        /// <param name="services">
        /// Zero or more service objects which should be made accessible to <see cref="MapperConfiguration"/>s
        /// via the GetService() method.
        /// </param>
        public void From<TConfiguration>(params object[] services)
            where TConfiguration : MapperConfiguration, new()
        {
            var configuration = new TConfiguration();

            Apply(configuration, services);
        }

        /// <summary>
        /// Apply all the <see cref="MapperConfiguration"/>s defined in the Assembly in which the given
        /// <typeparamref name="T">Type</typeparamref> is defined.
        /// </summary>
        /// <typeparam name="T">
        /// The type belonging to the Assembly in which to look for <see cref="MapperConfiguration"/>s.
        /// </typeparam>
        /// <param name="services">
        /// Zero or more service objects which should be made accessible to <see cref="MapperConfiguration"/>s
        /// via the GetService() method.
        /// </param>
        public void FromAssemblyOf<T>(params object[] services)
        {
            var configurations = typeof(T)
                .GetAssembly()
                .GetAllTypes()
                .Filter(t => !t.IsAbstract() && t.IsAssignableTo(typeof(MapperConfiguration)))
                .Project(t => (MapperConfiguration)Activator.CreateInstance(t));

            foreach (var configuration in configurations)
            {
                Apply(configuration, services);
            }
        }

        private void Apply(MapperConfiguration configuration, object[] services)
            => configuration.ApplyTo(_mapper, services);
    }
}