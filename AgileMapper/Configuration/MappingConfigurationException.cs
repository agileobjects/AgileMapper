namespace AgileObjects.AgileMapper.Configuration
{
    using System;

    /// <summary>
    /// Represents an error that occurred during mapping configuration.
    /// </summary>
    public class MappingConfigurationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the MappingConfigurationException class.
        /// </summary>
        public MappingConfigurationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the MappingConfigurationException class with the given 
        /// <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message that describes the mapping configuration error.</param>
        public MappingConfigurationException(string message)
            : base(message)
        {
        }
    }
}