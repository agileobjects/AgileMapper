namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
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
        /// <returns>
        /// The <see cref="MapperConfigurationSpecifier"/>, to enable further <see cref="MapperConfiguration"/>s
        /// to be registered.
        /// </returns>
        public MapperConfigurationSpecifier From<TConfiguration>(params object[] services)
            where TConfiguration : MapperConfiguration, new()
        {
            var configuration = new TConfiguration();

            Apply(configuration, services);
            return this;
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
        /// <returns>
        /// The <see cref="MapperConfigurationSpecifier"/>, to enable further <see cref="MapperConfiguration"/>s
        /// to be registered.
        /// </returns>
        public MapperConfigurationSpecifier FromAssemblyOf<T>(params object[] services)
        {
            ApplyConfigurationsIn(typeof(T).GetAssembly(), services);
            return this;
        }

        /// <summary>
        /// Apply all the <see cref="MapperConfiguration"/>s defined in the given <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="assemblies">
        /// One or more assemblies in which to look for <see cref="MapperConfiguration"/>s.
        /// </param>
        /// <param name="services">
        /// Zero or more service objects which should be made accessible to <see cref="MapperConfiguration"/>s
        /// via the GetService() method.
        /// </param>
        /// <returns>
        /// The <see cref="MapperConfigurationSpecifier"/>, to enable further <see cref="MapperConfiguration"/>s
        /// to be registered.
        /// </returns>
        public MapperConfigurationSpecifier From(IEnumerable<Assembly> assemblies, params object[] services)
            => From(assemblies, a => true, services);

        /// <summary>
        /// Apply all the <see cref="MapperConfiguration"/>s defined in the given <paramref name="assemblies"/>
        /// which match the given <paramref name="filter"/>.
        /// </summary>
        /// <param name="assemblies">
        /// One or more assemblies in which to look for <see cref="MapperConfiguration"/>s.
        /// </param>
        /// <param name="filter">
        /// A filter which assemblies should match before being checked for <see cref="MapperConfiguration"/>s.
        /// </param>
        /// <param name="services">
        /// Zero or more service objects which should be made accessible to <see cref="MapperConfiguration"/>s
        /// via the GetService() method.
        /// </param>
        /// <returns>
        /// The <see cref="MapperConfigurationSpecifier"/>, to enable further <see cref="MapperConfiguration"/>s
        /// to be registered.
        /// </returns>
        public MapperConfigurationSpecifier From(
            IEnumerable<Assembly> assemblies,
            Func<Assembly, bool> filter,
            params object[] services)
        {
            foreach (var assembly in assemblies.Filter(filter))
            {
                ApplyConfigurationsIn(assembly, services);
            }

            return this;
        }

        private void ApplyConfigurationsIn(Assembly assembly, ICollection<object> services)
        {
            var configurations = assembly
                .GetAllTypes()
                .Filter(t => !t.IsAbstract() && t.IsAssignableTo(typeof(MapperConfiguration)))
                .Project(t => (MapperConfiguration)Activator.CreateInstance(t));

            foreach (var configuration in configurations)
            {
                Apply(configuration, services);
            }
        }

        private void Apply(MapperConfiguration configuration, ICollection<object> services)
            => configuration.ApplyTo(_mapper, services);
    }
}