namespace AgileObjects.AgileMapper
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Members;
    using ReadableExpressions.Extensions;

    public class MappingException : Exception
    {
        internal static readonly ConstructorInfo ConstructorInfo =
            typeof(MappingException).GetConstructors(Constants.NonPublicInstance).First();

        public MappingException()
        {
        }

        internal MappingException(IMemberMappingContext context, Exception innerException)
            : base(GetMessage(context), innerException)
        {
        }

        private static string GetMessage(IMemberMappingContext context)
        {
            var rootData = GetRootMappingData(context);

            var sourcePath = GetMemberPath(rootData.SourceType, context.SourceMember, "Source");
            var targetPath = GetMemberPath(rootData.TargetType, context.TargetMember, "Target");

            return $"An exception occurred mapping {sourcePath} -> {targetPath} with rule set {context.RuleSetName}.";
        }

        private static IMappingData GetRootMappingData(IMappingData data)
        {
            while (data.Parent != null)
            {
                data = data.Parent;
            }

            return data;
        }

        private static string GetMemberPath(Type rootType, IQualifiedMember member, string rootMemberName)
        {
            var rootTargetType = rootType.GetFriendlyName();
            var memberPath = member.GetPath();

            var path = (memberPath != rootMemberName)
                ? rootTargetType + memberPath.Substring(rootMemberName.Length)
                : rootTargetType;

            return path;
        }
    }
}