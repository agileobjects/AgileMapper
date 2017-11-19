namespace AgileObjects.AgileMapper.Validation
{
    using System.Collections.Generic;
    using System.Linq;
    using Api.Validation;
    using Extensions;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;
    using static System.Environment;

    internal class MappingValidator : IMapperValidationSelector
    {
        private readonly IEnumerable<ObjectMapperData> _mapperDatas;

        public MappingValidator(Mapper mapper)
        {
            _mapperDatas = mapper.Context.ObjectMapperFactory.RootMappers.Select(m => m.MapperData);
        }

        public void MembersAreNotMapped()
        {
            var unmappedMemberData = _mapperDatas
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

                var sourceType = memberData.MapperData.SourceType.GetFriendlyName();
                var targetType = memberData.MapperData.TargetType.GetFriendlyName();

                failureMessage +=
                    sourceType + " -> " + targetType + NewLine +
                    "Rule set: " + memberData.MapperData.RuleSet.Name + NewLine + NewLine;

                foreach (var unmappedMember in memberData.UnmappedMembers)
                {
                    failureMessage += $" - {unmappedMember.Key.GetPath()} is unmapped." + NewLine;
                }
            }

            throw new MappingValidationException(failureMessage);
        }
    }
}
