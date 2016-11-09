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

        internal const string NO_MAPPING_DATA = "An exception occurred creating a mapping data instance";

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
                return NO_MAPPING_DATA;
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