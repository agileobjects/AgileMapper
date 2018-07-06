namespace AgileObjects.AgileMapper
{
    using System;
    using System.Reflection;
#if SERIALIZATION_SUPPORTED
    using System.Runtime.Serialization;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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
        private static readonly MethodInfo _factoryMethod =
            typeof(MappingException).GetPublicStaticMethod("For");

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

        private MappingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a new instance of the MappingException class.
        /// </summary>
        /// <param name="ruleSetName">The name of the mapping rule set being executed when the exception occurred.</param>
        /// <param name="sourcePath">The path of the source object being mapped when the exception occurred.</param>
        /// <param name="targetPath">The path of the target object being mapped when the exception occurred.</param>
        /// <param name="innerException">The exception which caused the creation of the MappingException.</param>
        /// <returns>A new MappingException instance.</returns>
        public static MappingException For(
            string ruleSetName,
            string sourcePath,
            string targetPath,
            Exception innerException)
        {
            return new MappingException(
                $"An exception occurred mapping {sourcePath} -> {targetPath} with rule set {ruleSetName}.",
                innerException);
        }

        internal static Expression GetFactoryMethodCall(IMemberMapperData mapperData, Expression exceptionVariable)
        {
            var rootData = mapperData.GetRootMapperData();
            var sourcePath = mapperData.SourceMember.GetFriendlySourcePath(rootData);
            var targetPath = mapperData.TargetMember.GetFriendlyTargetPath(rootData);

            var mappingExceptionCreation = Expression.Call(
                _factoryMethod,
                mapperData.RuleSet.NameConstant,
                sourcePath.ToConstantExpression(),
                targetPath.ToConstantExpression(),
                exceptionVariable);

            return mappingExceptionCreation;
        }
    }
}