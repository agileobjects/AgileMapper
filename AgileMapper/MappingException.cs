namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;

    /// <summary>
    /// Represents an error that occurred during a mapping.
    /// </summary>
    public class MappingException : Exception
    {
        internal static readonly MethodInfo FactoryMethod =
            typeof(MappingException).GetPublicStaticMethod("For");

        internal const string NoMappingData = "An exception occurred creating a mapping data instance";

        /// <summary>
        /// Initializes a new instance of the MappingException class.
        /// </summary>
        public MappingException()
        {
        }

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

            var rootData = GetRootMapperData(mapperData);

            var sourcePath = GetMemberPath(mapperData.SourceMember, rootData.SourceMember);
            var targetPath = GetMemberPath(mapperData.TargetMember, rootData.TargetMember);

            return $"An exception occurred mapping {sourcePath} -> {targetPath} with rule set {mapperData.RuleSet.Name}.";
        }

        private static IMemberMapperData GetRootMapperData(IMemberMapperData mapperData)
        {
            while (!mapperData.IsRoot)
            {
                mapperData = mapperData.Parent;
            }

            return mapperData;
        }

        private static string GetMemberPath(IQualifiedMember member, IQualifiedMember rootMember)
        {
            var rootTypeName = rootMember.Type.GetFriendlyName();
            var memberPath = member.GetPath();

            if (memberPath == rootMember.Name)
            {
                return rootTypeName;
            }

            if (memberPath.StartsWith(rootMember.Name, StringComparison.Ordinal))
            {
                return rootTypeName + memberPath.Substring(rootMember.Name.Length);
            }

            var rootMemberNameIndex = memberPath.IndexOf("." + rootMember.Name + ".", StringComparison.Ordinal);
            var rootMemberString = memberPath.Substring(rootMemberNameIndex + rootMember.Name.Length + 2);
            var path = rootTypeName + "." + rootMemberString;

            return path;
        }
    }
}