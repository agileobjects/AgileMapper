namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
    using System.Linq;

    internal class SourceMemberMatcher
    {
        public static IQualifiedMember GetMatchFor(IMemberMapperCreationData rootData)
        {
            var rootSourceMember = rootData.SourceMember;

            return GetAllSourceMembers(rootSourceMember, rootData)
                .FirstOrDefault(sm => IsMatchingMember(sm, rootData.MapperData));
        }

        private static IEnumerable<IQualifiedMember> GetAllSourceMembers(
            IQualifiedMember parentMember,
            IMemberMapperCreationData rootData)
        {
            yield return parentMember;

            if (!parentMember.CouldMatch(rootData.MapperData.TargetMember))
            {
                yield break;
            }

            var parentMemberType = rootData.GetSourceMemberRuntimeType(parentMember);

            if (parentMemberType != parentMember.Type)
            {
                // TODO: Add member runtime type conditions to RuleSetAndMembersKey if the runtime type determines the source member!
                parentMember = parentMember.WithType(parentMemberType);
                yield return parentMember;
            }

            var relevantMembers = GlobalContext
                .Instance
                .MemberFinder
                .GetReadableMembers(parentMember.Type)
                .Where(m => (m.IsSimple && rootData.TargetMember.IsSimple) || !m.IsSimple);

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

        private static bool IsMatchingMember(IQualifiedMember sourceMember, MemberMapperData data)
        {
            return sourceMember.Matches(data.TargetMember) &&
                   data.MapperContext.ValueConverters.CanConvert(sourceMember.Type, data.TargetMember.Type);
        }
    }
}