namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Extensions;
    using Members;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;

    /// <summary>
    /// Represents an error that occurred during a mapping.
    /// </summary>
    public class MappingException : Exception
    {
        internal static readonly ConstructorInfo ConstructorInfo =
            typeof(MappingException).GetNonPublicInstanceConstructors().First();

        /// <summary>
        /// Initializes a new instance of the MappingException class.
        /// </summary>
        public MappingException()
        {
        }

        internal MappingException(IObjectMappingData mappingData, Exception innerException)
            : base(GetMessage(mappingData?.MapperData), innerException)
        {
        }

        private static string GetMessage(IMemberMapperData mapperData)
        {
            if (mapperData == null)
            {
                return "An exception occurred creating a mapping data instance";
            }

            var rootData = GetRootMapperData(mapperData);

            var sourcePath = GetMemberPath(rootData.SourceType, mapperData.SourceMember, rootData.SourceMember.Name);
            var targetPath = GetMemberPath(rootData.TargetType, mapperData.TargetMember, rootData.TargetMember.Name);

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

        private static string GetMemberPath(Type rootType, IQualifiedMember member, string rootMemberName)
        {
            var rootTypeName = rootType.GetFriendlyName();
            var memberPath = member.GetPath();

            if (memberPath == rootMemberName)
            {
                return rootTypeName;
            }

            if (memberPath.StartsWith(rootMemberName, StringComparison.Ordinal))
            {
                return rootTypeName + memberPath.Substring(rootMemberName.Length);
            }

            var rootMemberNameIndex = memberPath.IndexOf("." + rootMemberName + ".", StringComparison.Ordinal);
            var rootMemberString = memberPath.Substring(rootMemberNameIndex + rootMemberName.Length + 2);
            var path = rootTypeName + "." + rootMemberString;

            return path;
        }
    }
}