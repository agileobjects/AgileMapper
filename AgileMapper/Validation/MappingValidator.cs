namespace AgileObjects.AgileMapper.Validation
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.Validation;
    using Configuration;
    using Extensions;
    using Members;
    using ObjectPopulation;
    using static System.Environment;

    internal class MappingValidator : IMapperValidationSelector, IMappingValidationSelector
    {
        private readonly IEnumerable<ObjectMapperData> _rootMapperDatas;

        public MappingValidator(Mapper mapper)
        {
            _rootMapperDatas = mapper.Context.ObjectMapperFactory.RootMappers.Select(m => m.MapperData);
        }

        public MappingValidator(MappingConfigInfo configInfo)
        {
            var creationCallbackKey = new MapperCreationCallbackKey(
                configInfo.RuleSet,
                configInfo.SourceType,
                configInfo.TargetType);

            configInfo.MapperContext.ObjectMapperFactory.RegisterCreationCallback(
                creationCallbackKey,
                createdMapper => MembersAreNotMapped(new[] { createdMapper.MapperData }));
        }

        void IMapperValidationSelector.MembersAreNotMapped() => MembersAreNotMapped(AllMapperDatas);

        void IMappingValidationSelector.MembersAreNotMapped()
        {

        }

        private static void MembersAreNotMapped(IEnumerable<ObjectMapperData> mapperDatas)
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
                .ToArray();

            if (unmappedMemberData.None())
            {
                return;
            }

            var failureMessage = string.Empty;

            foreach (var memberData in unmappedMemberData)
            {
                if (failureMessage.Length != 0)
                {
                    failureMessage += NewLine + NewLine;
                }

                var mapperData = memberData.MapperData;
                var rootData = mapperData.GetRootMapperData();
                var sourcePath = mapperData.SourceMember.GetFriendlySourcePath(rootData);
                var targetPath = mapperData.TargetMember.GetFriendlyTargetPath(rootData);

                failureMessage +=
                    sourcePath + " -> " + targetPath + NewLine +
                    "Rule set: " + mapperData.RuleSet.Name + NewLine + NewLine;

                foreach (var unmappedMember in memberData.UnmappedMembers)
                {
                    var targetMemberPath = unmappedMember.Key.GetFriendlyTargetPath(rootData);

                    failureMessage += $" - {targetMemberPath} is unmapped." + NewLine;
                }
            }

            throw new MappingValidationException(failureMessage);
        }

        private IEnumerable<ObjectMapperData> AllMapperDatas => GetAllMapperDatas(_rootMapperDatas);

        private IEnumerable<ObjectMapperData> GetAllMapperDatas(IEnumerable<ObjectMapperData> mapperDatas)
        {
            return mapperDatas.SelectMany(GetAllMapperDatas);
        }

        private IEnumerable<ObjectMapperData> GetAllMapperDatas(ObjectMapperData parent)
        {
            yield return parent;

            foreach (var childMapperData in GetAllMapperDatas(parent.ChildMapperDatas))
            {
                yield return childMapperData;
            }
        }
    }
}
