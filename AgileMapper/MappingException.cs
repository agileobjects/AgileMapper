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

        internal MappingException(IMappingData data, Exception innerException)
            : base(GetMessage(data), innerException)
        {
        }

        private static string GetMessage(IMappingData data)
        {
            var sourceTypeName = data.SourceType.GetFriendlyName();
            var rootTargetType = GetRootTargetType(data).GetFriendlyName();

            var targetPath = (data.TargetMember.Path != "Target")
                ? rootTargetType + data.TargetMember.Path.Substring("Target".Length)
                : rootTargetType;

            return $"An exception occurred mapping {sourceTypeName} -> {targetPath} with rule set {data.RuleSetName}.";
        }

        private static Type GetRootTargetType(IMappingData data)
        {
            while (data.Parent != null)
            {
                data = data.Parent;
            }

            return data.TargetType;
        }
    }
}