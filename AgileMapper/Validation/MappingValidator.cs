namespace AgileObjects.AgileMapper.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Api.Validation;
    using Extensions;
    using Members;
    using ObjectPopulation;
    using static System.Environment;

    internal class MappingValidator : IMapperValidationSelector
    {
        private readonly IEnumerable<ObjectMapperData> _rootMapperDatas;

        public MappingValidator(Mapper mapper)
        {
            _rootMapperDatas = mapper.Context.ObjectMapperFactory.RootMappers.Select(m => m.MapperData);
        }

        public void MembersAreNotMapped()
        {
            var unmappedMemberData = AllMapperDatas
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
