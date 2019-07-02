namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;
    using NetStandardPolyfills;

    internal static class SourceMemberMatcher
    {
        public static SourceMemberMatch GetMatchFor(
            IChildMemberMappingData targetMappingData,
            bool searchParentContexts = true)
        {
            var parentSourceMember = targetMappingData.MapperData.SourceMember;

            if (parentSourceMember.IsSimple)
            {
                return SourceMemberMatch.Null;
            }

            if (ExactMatchingMemberExists(parentSourceMember, targetMappingData, out var matchingMember) &&
                TypesAreCompatible(matchingMember.Type, targetMappingData.MapperData))
            {
                return new SourceMemberMatch(matchingMember, targetMappingData);
            }

            if (TryFindSourceMemberMatch(
                targetMappingData,
                parentSourceMember,
                ref matchingMember,
                out var sourceMemberMatch))
            {
                return sourceMemberMatch;
            }

            if (searchParentContexts &&
                TryFindParentContextSourceMemberMatch(targetMappingData, ref matchingMember, out sourceMemberMatch))
            {
                return sourceMemberMatch;
            }

            return (matchingMember != null)
                ? new SourceMemberMatch(matchingMember, targetMappingData, isUseable: false)
                : SourceMemberMatch.Null;
        }

        private static bool ExactMatchingMemberExists(
            IQualifiedMember parentSourceMember,
            IChildMemberMappingData targetData,
            out IQualifiedMember matchingMember)
        {
            var mapperData = targetData.MapperData;

            var matcher = mapperData.TargetType.IsAssignableTo(mapperData.SourceType)
                ? (Func<IChildMemberMappingData, Member, bool>)MembersAreTheSame
                : MembersMatch;

            var sourceMember = QuerySourceMembers(
                parentSourceMember,
                targetData,
                matcher)
                .FirstOrDefault();

            if (sourceMember == null)
            {
                matchingMember = null;
                return false;
            }

            matchingMember = parentSourceMember.Append(sourceMember);
            return true;
        }

        private static bool MembersAreTheSame(IChildMemberMappingData mappingData, Member sourceMember)
            => mappingData.MapperData.TargetMember.LeafMember.Equals(sourceMember);

        private static bool MembersMatch(IChildMemberMappingData mappingData, Member sourceMember)
        {
            if (MembersAreTheSame(mappingData, sourceMember))
            {
                return true;
            }

            return mappingData
                .MapperData
                .SourceMember
                .Append(sourceMember)
                .Matches(mappingData.MapperData.TargetMember);
        }

        private static IEnumerable<Member> QuerySourceMembers(
            IQualifiedMember parentMember,
            IChildMemberMappingData mappingData,
            Func<IChildMemberMappingData, Member, bool> filter)
        {
            var members = GlobalContext
                .Instance
                .MemberCache
                .GetSourceMembers(parentMember.Type)
                .Filter(m => filter.Invoke(mappingData, m));

            return mappingData.RuleSet.Settings.AllowGetMethods
                ? members
                : members.Filter(m => m.MemberType != MemberType.GetMethod);
        }

        private static bool TryFindParentContextSourceMemberMatch(
            IChildMemberMappingData targetMappingData,
            ref IQualifiedMember matchingMember,
            out SourceMemberMatch sourceMemberMatch)
        {
            var targetMember = targetMappingData.MapperData.TargetMember;
            var mappingData = targetMappingData.Parent;

            while (mappingData.Parent != null)
            {
                if (mappingData.MapperData.IsEntryPoint ||
                    mappingData.MapperData.TargetMemberIsEnumerableElement())
                {
                    break;
                }

                mappingData = mappingData.Parent;

                var childMapperData = new ChildMemberMapperData(targetMember, mappingData.MapperData);
                var contextMappingData = mappingData.GetChildMappingData(childMapperData);

                if (TryFindSourceMemberMatch(
                    targetMappingData,
                    mappingData.MapperData.SourceMember,
                    contextMappingData,
                    ref matchingMember,
                    out sourceMemberMatch))
                {
                    return true;
                }
            }

            sourceMemberMatch = null;
            return false;
        }

        private static bool TryFindSourceMemberMatch(
            IChildMemberMappingData targetMappingData,
            IQualifiedMember parentSourceMember,
            ref IQualifiedMember matchingMember,
            out SourceMemberMatch sourceMemberMatch)
        {
            return TryFindSourceMemberMatch(
                targetMappingData,
                parentSourceMember,
                targetMappingData,
                ref matchingMember,
                out sourceMemberMatch);
        }

        private static bool TryFindSourceMemberMatch(
            IChildMemberMappingData targetMappingData,
            IQualifiedMember parentSourceMember,
            IChildMemberMappingData contextMappingData,
            ref IQualifiedMember matchingMember,
            out SourceMemberMatch sourceMemberMatch)
        {
            var candidateSourceMembers = EnumerateSourceMembers(parentSourceMember, contextMappingData)
                .Where(targetMappingData.MapperData.TargetMember.Matches);

            foreach (var sourceMember in candidateSourceMembers)
            {
                if (TypesAreCompatible(sourceMember.Type, targetMappingData.MapperData))
                {
                    sourceMemberMatch = new SourceMemberMatch(sourceMember, contextMappingData);
                    return true;
                }

                if (matchingMember == null)
                {
                    matchingMember = sourceMember;
                }
            }

            sourceMemberMatch = null;
            return false;
        }

        private static IEnumerable<IQualifiedMember> EnumerateSourceMembers(
            IQualifiedMember parentMember,
            IChildMemberMappingData rootData)
        {
            yield return parentMember;

            if (!parentMember.CouldMatch(rootData.MapperData.TargetMember))
            {
                yield break;
            }

            var parentMemberType = rootData.Parent.GetSourceMemberRuntimeType(parentMember);

            if (parentMemberType != parentMember.Type)
            {
                parentMember = parentMember.WithType(parentMemberType);
                yield return parentMember;

                if (parentMember.IsSimple)
                {
                    yield break;
                }
            }

            var relevantSourceMembers = QuerySourceMembers(
                parentMember,
                rootData,
                MembersHaveCompatibleTypes);

            foreach (var sourceMember in relevantSourceMembers)
            {
                var childMember = parentMember.Append(sourceMember);

                if (sourceMember.IsSimple)
                {
                    yield return childMember;
                    continue;
                }

                foreach (var qualifiedMember in EnumerateSourceMembers(childMember, rootData))
                {
                    yield return qualifiedMember;
                }
            }
        }

        private static bool MembersHaveCompatibleTypes(IChildMemberMappingData rootData, Member sourceMember)
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

        private static bool TypesAreCompatible(Type sourceType, IMemberMapperData mapperData)
            => mapperData.CanConvert(sourceType, mapperData.TargetMember.Type);
    }
}