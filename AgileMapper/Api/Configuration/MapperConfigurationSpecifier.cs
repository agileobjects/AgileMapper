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
        public void From<TConfiguration>()
            where TConfiguration : MapperConfiguration, new()
        {
            var configuration = new TConfiguration();

            configuration.ApplyTo(_mapper);
        }

        /// <summary>
        /// Apply all the <see cref="MapperConfiguration"/>s defined in the Assembly in which the given
        /// <typeparamref name="T">Type</typeparamref> is defined.
        /// </summary>
        /// <typeparam name="T">
        /// The type belonging to the Assembly in which to look for <see cref="MapperConfiguration"/>s.
        /// </typeparam>
        public void FromAssemblyOf<T>()
        {
            var configurations = typeof(T)
                .GetAssembly()
                .GetAllTypes()
                .Filter(t => !t.IsAbstract() && t.IsAssignableTo(typeof(MapperConfiguration)))
                .Project(t => (MapperConfiguration)Activator.CreateInstance(t));

            foreach (var configuration in configurations)
            {
                configuration.ApplyTo(_mapper);
            }
        }
    }
}