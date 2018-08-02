namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AgileMapper.Configuration;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    /// <summary>
    /// Provides options for specifying <see cref="MapperConfiguration"/> instances with which to
    /// perform Mapper configuration.
    /// </summary>
    public class MapperConfigurationSpecifier
    {
        private readonly IMapperInternal _mapper;

        internal MapperConfigurationSpecifier(IMapperInternal mapper)
        {
            _mapper = mapper;
        }

#if !NET_STANDARD
        /// <summary>
        /// Apply all the <see cref="MapperConfiguration"/>s defined in the Assemblies loaded into the current
        /// AppDomain.
        /// </summary>
        /// <returns>
        /// The <see cref="MapperConfigurationSpecifier"/>, to enable further <see cref="MapperConfiguration"/>s
        /// to be registered.
        /// </returns>
        public MapperConfigurationSpecifier FromCurrentAppDomain()
            => FromCurrentAppDomain(AllAssemblies);

        /// <summary>
        /// Apply all the <see cref="MapperConfiguration"/>s defined in the Assemblies loaded into the current
        /// AppDomain.
        /// </summary>
        /// <param name="filter">
        /// A filter which assemblies should match before being checked for <see cref="MapperConfiguration"/>s.
        /// </param>
        /// <returns>
        /// The <see cref="MapperConfigurationSpecifier"/>, to enable further <see cref="MapperConfiguration"/>s
        /// to be registered.
        /// </returns>
        public MapperConfigurationSpecifier FromCurrentAppDomain(Func<Assembly, bool> filter)
            => From(AppDomain.CurrentDomain.GetAssemblies(), filter);
#endif
        /// <summary>
        /// Apply all the <see cref="MapperConfiguration"/>s defined in the given <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="assemblies">
        /// One or more assemblies in which to look for <see cref="MapperConfiguration"/>s.
        /// </param>
        /// <returns>
        /// The <see cref="MapperConfigurationSpecifier"/>, to enable further <see cref="MapperConfiguration"/>s
        /// to be registered.
        /// </returns>
        public MapperConfigurationSpecifier From(IEnumerable<Assembly> assemblies)
            => From(assemblies, AllAssemblies);

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
        /// <returns>
        /// The <see cref="MapperConfigurationSpecifier"/>, to enable further <see cref="MapperConfiguration"/>s
        /// to be registered.
        /// </returns>
        public MapperConfigurationSpecifier From(IEnumerable<Assembly> assemblies, Func<Assembly, bool> filter)
        {
            ThrowIfInvalidAssembliesSupplied(assemblies != null, nullSupplied: true);

            var matchingAssemblies = assemblies.Filter(filter).ToArray();

            ThrowIfInvalidAssembliesSupplied(matchingAssemblies.Any(), nullSupplied: false);

            ApplyConfigurationsIn(matchingAssemblies.SelectMany(QueryConfigurationTypesIn));
            return this;
        }

        /// <summary>
        /// Apply the configuration in the <see cref="MapperConfiguration"/> of the given
        /// <typeparamref name="TConfiguration"/> instance.
        /// </summary>
        /// <typeparam name="TConfiguration">The <see cref="MapperConfiguration"/> implementation to apply.</typeparam>
        /// <returns>
        /// The <see cref="MapperConfigurationSpecifier"/>, to enable further <see cref="MapperConfiguration"/>s
        /// to be registered.
        /// </returns>
        public MapperConfigurationSpecifier From<TConfiguration>()
            where TConfiguration : MapperConfiguration, new()
        {
            var configuration = new TConfiguration();

            Apply(configuration);
            return this;
        }

        /// <summary>
        /// Apply all the <see cref="MapperConfiguration"/>s defined in the Assembly in which the given
        /// <typeparamref name="T">Type</typeparamref> is defined.
        /// </summary>
        /// <typeparam name="T">
        /// The type belonging to the Assembly in which to look for <see cref="MapperConfiguration"/>s.
        /// </typeparam>
        /// <returns>
        /// The <see cref="MapperConfigurationSpecifier"/>, to enable further <see cref="MapperConfiguration"/>s
        /// to be registered.
        /// </returns>
        public MapperConfigurationSpecifier FromAssemblyOf<T>()
        {
            ApplyConfigurationsIn(QueryConfigurationTypesIn(typeof(T).GetAssembly()));
            return this;
        }

        private void ApplyConfigurationsIn(IEnumerable<Type> configurationTypes)
        {
            var configurationData = configurationTypes
                .Select(t => new ConfigurationData(t))
                .ToList();

            var configurationCount = configurationData.Count;
            var orderedConfigurations = new List<MapperConfiguration>(configurationCount);

            for (var i = 0; i < configurationCount; i++)
            {
                if (configurationData[i].ApplyAfter.None())
                {
                    orderedConfigurations.Add(configurationData[i].Configuration);
                    configurationData.RemoveAt(i);
                }
            }

            if (configurationData.None())
            {
                Apply(orderedConfigurations);
                return;
            }

            foreach (var configurationItem in configurationData)
            {
                if (configurationItem.Index > -1)
                {
                    // Already added by virtue of another configuration
                    // depending on it:
                    continue;
                }
              
                InsertInOrder(configurationItem, configurationData, orderedConfigurations);
            }
        }

        private static void InsertInOrder(
            ConfigurationData configurationItem,
            ICollection<ConfigurationData> allConfigurationData,
            ICollection<MapperConfiguration> orderedConfigurations)
        {
            // Get largest Index of any ApplyAfter dependencies
            // InsertAfter(largest index)

                
        }

        private void Apply(IEnumerable<MapperConfiguration> configurations)
        {
            foreach (var configuration in configurations)
            {
                Apply(configuration);
            }
        }

        private static IEnumerable<Type> QueryConfigurationTypesIn(Assembly assembly)
        {
            ThrowIfInvalidAssemblySupplied(assembly);

            return assembly
                .QueryTypes()
                .Filter(t => !t.IsAbstract() && t.IsDerivedFrom(typeof(MapperConfiguration)));
        }

        private void Apply(MapperConfiguration configuration)
        {
            try
            {
                configuration.ApplyTo(_mapper);
            }
            catch (Exception ex)
            {
                throw new MappingConfigurationException(
                    $"Exception encountered while applying configuration from {configuration.GetType().GetFriendlyName()}.",
                    ex);
            }
        }

        private static void ThrowIfInvalidAssembliesSupplied(bool isValid, bool nullSupplied)
        {
            if (isValid)
            {
                return;
            }

            throw nullSupplied
                ? NullAssemblySupplied("Assemblies cannot be null")
                : new MappingConfigurationException("Assemblies cannot be empty");
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void ThrowIfInvalidAssemblySupplied(Assembly assembly)
        {
            if (assembly == null)
            {
                throw NullAssemblySupplied("All supplied assemblies must be non-null");
            }
        }

        private static MappingConfigurationException NullAssemblySupplied(string message)
        {
            // ReSharper disable once NotResolvedInText
            return new MappingConfigurationException(message, new ArgumentNullException("assemblies"));
        }

        private static bool AllAssemblies(Assembly assembly) => true;

        private class ConfigurationData
        {
            public ConfigurationData(Type configurationType)
            {
                ConfigurationType = configurationType;
                Configuration = (MapperConfiguration)Activator.CreateInstance(configurationType);
                ApplyAfter = configurationType.GetAttributes<ApplyAfterAttribute>().ToArray();
                Index = -1;
            }

            public Type ConfigurationType { get; }

            public MapperConfiguration Configuration { get; }

            public ApplyAfterAttribute[] ApplyAfter { get; }

            public bool HasBeenAdded => Index > -1;

            public int Index { get; }
        }
    }
}