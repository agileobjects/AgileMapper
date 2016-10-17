namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
    using System.Linq;

    internal class SourceMemberMatcher
    {
        public static IQualifiedMember GetMatchFor(IMemberMappingData rootData)
        {
            var rootSourceMember = rootData.MapperData.SourceMember;

            return GetAllSourceMembers(rootSourceMember, rootData)
                .FirstOrDefault(sm => IsMatchingMember(sm, rootData.MapperData));
        }

        private static IEnumerable<IQualifiedMember> GetAllSourceMembers(
            IQualifiedMember parentMember,
            IMemberMappingData rootData)
        {
            yield return parentMember;

            if (!parentMember.CouldMatch(rootData.MapperData.TargetMember))
            {
                yield break;
            }

            var parentMemberType = rootData.GetSourceMemberRuntimeType(parentMember);

            if (parentMemberType != parentMember.Type)
            {
                parentMember = parentMember.WithType(parentMemberType);
                yield return parentMember;
            }

            var relevantMembers = GlobalContext
                .Instance
                .MemberFinder
                .GetReadableMembers(parentMember.Type)
                .Where(m => (m.IsSimple && rootData.MapperData.TargetMember.IsSimple) || !m.IsSimple);

            foreach (var sourceMember in relevantMembers)
            {
                var childMember = parentMember.Append(sourceMember);

                if (sourceMember.IsSimple)
                {
                    yield return childMember;
                    continue;
                }

                foreach (var qualifiedMember in GetAllSourceMembers(childMember, rootData))
                {
                    yield return qualifiedMember;
                }
            }
        }

        private static bool IsMatchingMember(IQualifiedMember sourceMember, IMemberMapperData mapperData)
        {
            return sourceMember.Matches(mapperData.TargetMember) &&
                   mapperData.MapperContext.ValueConverters.CanConvert(sourceMember.Type, mapperData.TargetMember.Type);
        }
    }
}