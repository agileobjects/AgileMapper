namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET_STANDARD
    using System.Reflection;
#endif

    internal static class SourceMemberMatcher
    {
        public static IQualifiedMember GetMatchFor(IChildMemberMappingData targetData)
        {
            var parentSourceMember = targetData.MapperData.SourceMember;

            IQualifiedMember matchingMember;

            if (SourceHasSameMemberAsTarget(parentSourceMember, targetData))
            {
                var matchingSourceMember = GetSourceMembers(
                    parentSourceMember,
                    m => m.Name == targetData.MapperData.TargetMember.Name).First();

                matchingMember = parentSourceMember.Append(matchingSourceMember);

                if (!TypesAreCompatible(matchingMember, targetData.MapperData))
                {
                    return null;
                }
            }
            else
            {
                matchingMember = GetAllSourceMembers(parentSourceMember, targetData)
                    .FirstOrDefault(sm => IsMatchingMember(sm, targetData.MapperData));

                if (matchingMember == null)
                {
                    return null;
                }
            }

            return targetData.MapperData
                .MapperContext
                .QualifiedMemberFactory
                .GetFinalSourceMember(matchingMember, targetData.MapperData.TargetMember);
        }

        private static bool SourceHasSameMemberAsTarget(
            IQualifiedMember parentSourceMember,
            IChildMemberMappingData targetData)
        {
            return targetData.MapperData.TargetMember.IsReadable &&
                   targetData.Parent.MapperData.TargetType.IsAssignableFrom(parentSourceMember.Type);
        }

        private static IEnumerable<Member> GetSourceMembers(
            IQualifiedMember parentMember,
            Func<Member, bool> filter)
        {
            return GlobalContext
                .Instance
                .MemberFinder
                .GetSourceMembers(parentMember.Type)
                .Where(filter);
        }

        private static IEnumerable<IQualifiedMember> GetAllSourceMembers(
            IQualifiedMember parentMember,
            IChildMemberMappingData rootData)
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

            var relevantSourceMembers = GetSourceMembers(
                parentMember,
                sourceMember => MembersHaveCompatibleTypes(sourceMember, rootData));

            foreach (var sourceMember in relevantSourceMembers)
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

        private static bool MembersHaveCompatibleTypes(Member sourceMember, IChildMemberMappingData rootData)
        {
            if (!sourceMember.IsSimple)
            {
                return true;
            }

            var targetMember = rootData.MapperData.TargetMember;

            if (targetMember.IsSimple)
            {
                return true;
            }

            return targetMember.Type == typeof(object);
        }

        private static bool IsMatchingMember(IQualifiedMember sourceMember, IMemberMapperData mapperData)
        {
            return sourceMember.Matches(mapperData.TargetMember) && TypesAreCompatible(sourceMember, mapperData);
        }

        private static bool TypesAreCompatible(IQualifiedMember sourceMember, IMemberMapperData mapperData)
        {
            return mapperData.MapperContext.ValueConverters.CanConvert(sourceMember.Type, mapperData.TargetMember.Type);
        }
    }
}