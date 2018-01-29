namespace AgileObjects.AgileMapper.Validation
{
    using System;
    #region Serialization Support
#if SERIALIZATION_SUPPORTED
    using System.Runtime.Serialization;
#endif
    #endregion

    /// <summary>
    /// Provides details of a mapping validation failure.
    /// </summary>
    #region Serialization Support
#if SERIALIZATION_SUPPORTED
    [Serializable]
#endif
    #endregion
    public class MappingValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the MappingValidationException class.
        /// </summary>
        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public MappingValidationException()
            : this("Mapping validation failed.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the MappingValidationException class with the given <paramref name="message"/>.
        /// </summary>
        /// <param name="message">The message detailing the mapping validation failure.</param>
        public MappingValidationException(string message)
            : base(message)
        {
        }

        #region Serialization Support
#if SERIALIZATION_SUPPORTED
        /// <summary>
        /// Initializes a new instance of the MappingValidationException class. This constructor is provided
        /// to support deserialization.
        /// </summary>
        /// <param name="info">The SerializationInfo containing serialization information.</param>
        /// <param name="context">The StreamingContext in which the deserialization is being performed.</param>
        // ReSharper disable UnusedParameter.Local
        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        protected MappingValidationException(SerializationInfo info, StreamingContext context)
        {
        }
        // ReSharper restore UnusedParameter.Local
#endif
        #endregion
    }
}