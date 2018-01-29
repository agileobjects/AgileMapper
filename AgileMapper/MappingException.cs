namespace AgileObjects.AgileMapper
{
    using System;
    using System.Reflection;
#if SERIALIZATION_SUPPORTED
    using System.Runtime.Serialization;
#endif
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;

    /// <summary>
    /// Represents an error that occurred during a mapping.
    /// </summary>
    #region Serialization Support
#if SERIALIZATION_SUPPORTED
    [Serializable]
#endif
    #endregion
    public class MappingException : Exception
    {
        internal static readonly MethodInfo FactoryMethod =
            typeof(MappingException).GetPublicStaticMethod("For");

        internal const string NoMappingData = "An exception occurred creating a mapping data instance";

        #region Serialization Support
#if SERIALIZATION_SUPPORTED
        /// <summary>
        /// Initializes a new instance of the MappingException class. This constructor is provided
        /// to support deserialization.
        /// </summary>
        /// <param name="info">The SerializationInfo containing serialization information.</param>
        /// <param name="context">The StreamingContext in which the deserialization is being performed.</param>
        // ReSharper disable UnusedParameter.Local
        protected MappingException(SerializationInfo info, StreamingContext context)
        {
        }
        // ReSharper restore UnusedParameter.Local
#endif
        #endregion

        private MappingException(IMemberMapperData mapperData, Exception innerException)
            : base(GetMessage(mapperData), innerException)
        {
        }

        /// <summary>
        /// Creates a new instance of the MappingException class.
        /// </summary>
        /// <typeparam name="TSource">The source type being mapped when the exception occurred.</typeparam>
        /// <typeparam name="TTarget">The target type being mapped when the exception occurred.</typeparam>
        /// <param name="mappingData">
        /// The <see cref="IObjectMappingData{TSource, TTarget}"/> containing the mapping data of the 
        /// current mapping context.
        /// </param>
        /// <param name="innerException">The exception which caused the creation of the MappingException.</param>
        /// <returns>A new MappingException instance.</returns>
        public static MappingException For<TSource, TTarget>(
            IObjectMappingData<TSource, TTarget> mappingData,
            Exception innerException)
        {
            return new MappingException(((IObjectMappingData)mappingData)?.MapperData, innerException);
        }

        private static string GetMessage(IMemberMapperData mapperData)
        {
            if (mapperData == null)
            {
                return NoMappingData;
            }

            var rootData = mapperData.GetRootMapperData();
            var sourcePath = mapperData.SourceMember.GetFriendlySourcePath(rootData);
            var targetPath = mapperData.TargetMember.GetFriendlyTargetPath(rootData);

            return $"An exception occurred mapping {sourcePath} -> {targetPath} with rule set {mapperData.RuleSet.Name}.";
        }
    }
}