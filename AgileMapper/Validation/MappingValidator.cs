namespace AgileObjects.AgileMapper.Validation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Configuration;
    using Extensions;
    using Members;
    using ObjectPopulation;

    internal static class MappingValidator
    {
        public static void Validate(Mapper mapper)
        {
            var rootMapperDatas = mapper.Context.ObjectMapperFactory.RootMappers.Select(m => m.MapperData);

            VerifyAllTargetMembersAreMapped(GetAllMapperDatas(rootMapperDatas));
        }

        public static void Validate(MappingConfigInfo configInfo)
        {
            var creationCallbackKey = new MapperCreationCallbackKey(
                configInfo.RuleSet,
                configInfo.SourceType,
                configInfo.TargetType);

            configInfo.MapperContext.ObjectMapperFactory.RegisterCreationCallback(
                creationCallbackKey,
                createdMapper => VerifyAllTargetMembersAreMapped(new[] { createdMapper.MapperData }));
        }

        private static void VerifyAllTargetMembersAreMapped(IEnumerable<ObjectMapperData> mapperDatas)
        {
            var unmappedMemberData = mapperDatas
                .Select(md => new
                {
                    MapperData = md,
                    UnmappedMembers = md
                        .DataSourcesByTargetMember
                        .Where(pair => !pair.Value.HasValue)
                        .Select(pair => pair)
                        .ToArray()
                })
                .Where(d => d.UnmappedMembers.Any())
                .GroupBy(d => d.MapperData.GetRootMapperData())
                .Select(g => new
                {
                    RootMapperData = g.Key,
                    UnmappedMembers = g.SelectMany(d => d.UnmappedMembers).ToArray()
                })
                .ToArray();

            if (unmappedMemberData.None())
            {
                return;
            }

            var previousRootMapperData = default(IMemberMapperData);

            var failureMessage = new StringBuilder();

            foreach (var memberData in unmappedMemberData)
            {
                if (failureMessage.Length != 0)
                {
                    failureMessage.AppendLine().AppendLine();
                }

                var rootData = memberData.RootMapperData;
                var sourcePath = rootData.SourceMember.GetFriendlySourcePath(rootData);
                var targetPath = rootData.TargetMember.GetFriendlyTargetPath(rootData);

                if ((previousRootMapperData?.SourceType != rootData.SourceType) ||
                    (previousRootMapperData?.TargetType != rootData.TargetType))
                {
                    failureMessage
                        .AppendLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -")
                        .AppendLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -")
                        .Append("- ").Append(sourcePath).Append(" -> ").AppendLine(targetPath)
                        .AppendLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -")
                        .AppendLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -")
                        .AppendLine();

                    previousRootMapperData = rootData;
                }

                failureMessage
                    .Append(" Rule set: ").AppendLine(rootData.RuleSet.Name).AppendLine()
                    .AppendLine(" Unmapped target members - fix by ignoring or configuring a custom source member or data source:").AppendLine();

                foreach (var unmappedMember in memberData.UnmappedMembers)
                {
                    var targetMemberPath = unmappedMember.Key.GetFriendlyTargetPath(rootData);

                    failureMessage.Append("  - ").AppendLine(targetMemberPath);
                }
            }

            throw new MappingValidationException(failureMessage.ToString());
        }

        private static IEnumerable<ObjectMapperData> GetAllMapperDatas(IEnumerable<ObjectMapperData> mapperDatas)
        {
            return mapperDatas.SelectMany(GetAllMapperDatas);
        }

        private static IEnumerable<ObjectMapperData> GetAllMapperDatas(ObjectMapperData parent)
        {
            yield return parent;

            foreach (var childMapperData in GetAllMapperDatas(parent.ChildMapperDatas))
            {
                yield return childMapperData;
            }
        }
    }
}
