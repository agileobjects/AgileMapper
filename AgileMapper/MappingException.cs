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

        internal MappingException(IObjectMappingContextData data, Exception innerException)
            : base(GetMessage(data.MapperData), innerException)
        {
        }

        private static string GetMessage(MemberMapperData data)
        {
            var rootData = GetRootMapperData(data);

            var sourcePath = GetMemberPath(rootData.SourceType, data.SourceMember, "Source");
            var targetPath = GetMemberPath(rootData.TargetType, data.TargetMember, "Target");

            return $"An exception occurred mapping {sourcePath} -> {targetPath} with rule set {data.RuleSet.Name}.";
        }

        private static IBasicMapperData GetRootMapperData(IBasicMapperData data)
        {
            while (data.Parent != null)
            {
                data = data.Parent;
            }

            return data;
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

            var path = memberPath.Replace("data." + rootMemberName + ".", rootTypeName + ".");

            return path;
        }
    }
}