namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
    using System.Linq;

    internal class SourceMemberMatcher
    {
        public static IQualifiedMember GetMatchFor(IMemberMappingContext context)
        {
            var rootSourceMember = context.SourceMember;

            return GetAllSourceMembers(rootSourceMember, context)
                .FirstOrDefault(sm => IsMatchingMember(sm, context));
        }

        private static IEnumerable<IQualifiedMember> GetAllSourceMembers(
            IQualifiedMember parentMember,
            IMemberMappingContext context)
        {
            yield return parentMember;

            if (!parentMember.CouldMatch(context.TargetMember))
            {
                yield break;
            }

            var parentMemberType = context.Parent.GetSourceMemberRuntimeType(parentMember);

            if (parentMemberType != parentMember.Type)
            {
                parentMember = parentMember.WithType(parentMemberType);
                yield return parentMember;
            }

            var relevantMembers = context.Parent
                .GlobalContext
                .MemberFinder
                .GetReadableMembers(parentMember.Type)
                .Where(m => (m.IsSimple && context.TargetMember.IsSimple) || !m.IsSimple);

            foreach (var sourceMember in relevantMembers)
            {
                var childMember = parentMember.Append(sourceMember);

                if (sourceMember.IsSimple)
                {
                    yield return childMember;
                    continue;
                }

                foreach (var qualifiedMember in GetAllSourceMembers(childMember, context))
                {
                    yield return qualifiedMember;
                }
            }
        }

        private static bool IsMatchingMember(IQualifiedMember sourceMember, IMemberMappingContext context)
        {
            return sourceMember.Matches(context.TargetMember) &&
                   context.MapperContext.ValueConverters.CanConvert(sourceMember.Type, context.TargetMember.Type);
        }
    }
}