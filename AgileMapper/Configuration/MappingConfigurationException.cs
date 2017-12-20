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
        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public MappingConfigurationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the MappingConfigurationException class with the given 
        /// <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message that describes the mapping configuration error.</param>
        public MappingConfigurationException(string message)
            : this(message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the MappingConfigurationException class with the given 
        /// <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message that describes the mapping configuration error.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or null if no inner 
        /// exception exists.
        /// </param>
        public MappingConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}