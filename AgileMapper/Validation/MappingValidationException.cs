namespace AgileObjects.AgileMapper.Validation
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Provides details of a mapping validation failure.
    /// </summary>
    [Serializable]
    public class MappingValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the MappingValidationException class.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the MappingValidationException class. This constructor is provided
        /// to support deserialization.
        /// </summary>
        /// <param name="info">The SerializationInfo containing serialization information.</param>
        /// <param name="context">The StreamingContext in which the deserialization is being performed.</param>
        // ReSharper disable UnusedParameter.Local
        protected MappingValidationException(SerializationInfo info, StreamingContext context)
        {
        }
        // ReSharper restore UnusedParameter.Local
    }
}