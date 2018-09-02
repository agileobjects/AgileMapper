namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using AgileMapper.Configuration;
    using Extensions;
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
            ThrowIfConfigurationAlreadyApplied(typeof(TConfiguration));
            ThrowIfDependedOnConfigurationNotApplied(typeof(TConfiguration));

            var configuration = new TConfiguration();

            Apply(configuration);
            return this;
        }

        private void ThrowIfConfigurationAlreadyApplied(Type configurationType)
        {
            if (ConfigurationApplied(configurationType))
            {
                throw new MappingConfigurationException(
                    $"MapperConfiguration {configurationType.GetFriendlyName()} has already been applied");
            }
        }

        private void ThrowIfDependedOnConfigurationNotApplied(Type configurationType)
        {
            var dependedOnTypes = GetDependedOnConfigurationTypesFor(configurationType);

            if (dependedOnTypes.None())
            {
                return;
            }

            var missingDependencies = dependedOnTypes
                .Filter(t => !ConfigurationApplied(t))
                .ToArray();

            if (missingDependencies.None())
            {
                return;
            }

            var configurationTypeName = configurationType.GetFriendlyName();
            var dependencyNames = missingDependencies.Project(d => d.GetFriendlyName()).Join(", ");

            throw new MappingConfigurationException(
                $"Configuration {configurationTypeName} must be registered after depended-on configuration(s) {dependencyNames}");
        }

        private bool ConfigurationApplied(Type configurationType)
            => _mapper.Context.UserConfigurations.AppliedConfigurationTypes.Contains(configurationType);

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

            if (configurationData.None(d => d.DependedOnConfigurationTypes.Any()))
            {
                Apply(configurationData.Project(d => d.Configuration));
                return;
            }

            var configurationCount = configurationData.Count;
            var configurationDataByType = configurationData.ToDictionary(d => d.ConfigurationType);
            var configurationIndexesByType = new Dictionary<Type, int>(configurationCount);

            var configurationIndex = -1;

            for (var i = configurationCount - 1; i >= 0; --i)
            {
                if (configurationData[i].DependedOnConfigurationTypes.None())
                {
                    configurationIndexesByType.Add(configurationData[i].ConfigurationType, ++configurationIndex);
                    configurationData.RemoveAt(i);
                }
            }

            var checkedTypes = new List<Type>(configurationCount + 1);

            foreach (var configurationItem in configurationData)
            {
                if (configurationIndexesByType.ContainsKey(configurationItem.ConfigurationType))
                {
                    // Already added by virtue of another configuration
                    // depending on it:
                    continue;
                }

                InsertWithOrder(
                    configurationItem,
                    configurationIndexesByType,
                    configurationDataByType,
                    checkedTypes);
            }

            var orderedConfigurations = configurationIndexesByType
                .OrderBy(kvp => kvp.Value)
                .Project(kvp => configurationDataByType[kvp.Key].Configuration);

            Apply(orderedConfigurations);
        }

        private static void InsertWithOrder(
            ConfigurationData configurationItem,
            IDictionary<Type, int> configurationIndexesByType,
            IDictionary<Type, ConfigurationData> configurationDataByType,
            ICollection<Type> checkedTypes)
        {
            ThrowIfCircularDependencyDetected(configurationItem, checkedTypes);

            var typeIndex = -1;

            foreach (var configurationType in configurationItem.DependedOnConfigurationTypes)
            {
                var index = GetIndexOf(
                    configurationType,
                    configurationIndexesByType,
                    configurationDataByType,
                    checkedTypes);

                if (index > typeIndex)
                {
                    typeIndex = index + 1;
                }
            }

            while (configurationIndexesByType.Values.Contains(typeIndex))
            {
                ++typeIndex;
            }

            configurationIndexesByType.Add(configurationItem.ConfigurationType, typeIndex);
        }

        private static void ThrowIfCircularDependencyDetected(ConfigurationData configurationItem, ICollection<Type> checkedTypes)
        {
            if (!checkedTypes.Contains(configurationItem.ConfigurationType))
            {
                checkedTypes.Add(configurationItem.ConfigurationType);
                return;
            }

            checkedTypes.Add(configurationItem.ConfigurationType);

            var dependencies = checkedTypes
                .Project(configurationType => configurationType.GetFriendlyName())
                .Join(" > ");

            throw new MappingConfigurationException(
                $"Circular dependency detected in {nameof(MapperConfiguration)}s: {dependencies}");
        }

        private static int GetIndexOf(
            Type configurationType,
            IDictionary<Type, int> configurationIndexesByType,
            IDictionary<Type, ConfigurationData> configurationDataByType,
            ICollection<Type> checkedTypes)
        {
            if (configurationIndexesByType.TryGetValue(configurationType, out var index))
            {
                return index;
            }

            InsertWithOrder(
                configurationDataByType[configurationType],
                configurationIndexesByType,
                configurationDataByType,
                checkedTypes);

            return configurationIndexesByType[configurationType];
        }

        private static IEnumerable<Type> QueryConfigurationTypesIn(Assembly assembly)
        {
            ThrowIfInvalidAssemblySupplied(assembly);

            return assembly
                .QueryTypes()
                .Filter(t => !t.IsAbstract() && t.IsDerivedFrom(typeof(MapperConfiguration)));
        }

        private void Apply(IEnumerable<MapperConfiguration> configurations)
        {
            foreach (var configuration in configurations)
            {
                Apply(configuration);
            }
        }

        private void Apply(MapperConfiguration configuration)
        {
            try
            {
                configuration.ApplyTo(_mapper);
            }
            catch (Exception ex)
            {
                var configurationName = configuration.GetType().GetFriendlyName();

                throw new MappingConfigurationException(
                    $"Exception encountered while applying configuration from {configurationName}.",
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
                DependedOnConfigurationTypes = GetDependedOnConfigurationTypesFor(configurationType);
            }

            public Type ConfigurationType { get; }

            public MapperConfiguration Configuration { get; }

            public Type[] DependedOnConfigurationTypes { get; }
        }

        private static Type[] GetDependedOnConfigurationTypesFor(Type configurationType)
        {
            return configurationType
                .GetAttributes<ApplyAfterAttribute>()
                .SelectMany(attr => attr.PreceedingMapperConfigurationTypes)
                .Distinct()
                .ToArray();
        }
    }
}