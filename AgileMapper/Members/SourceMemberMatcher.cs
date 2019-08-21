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
            var targetMapperData = targetMappingData.MapperData;
            var parentSourceMember = targetMapperData.SourceMember;

            if (parentSourceMember.IsSimple)
            {
                return SourceMemberMatch.Null;
            }

            if (ExactMatchingMemberExists(parentSourceMember, targetMapperData, out var matchingMember) &&
                TypesAreCompatible(matchingMember.Type, targetMapperData))
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
            IMemberMapperData mapperData,
            out IQualifiedMember matchingMember)
        {
            var matcher = mapperData.TargetType.IsAssignableTo(mapperData.SourceType)
                ? (Func<IMemberMapperData, Member, bool>)MembersAreTheSame
                : MembersMatch;

            matchingMember = QuerySourceMembers(
                parentSourceMember,
                mapperData,
                matcher)
               .FirstOrDefault();

            return matchingMember != null;
        }

        private static bool MembersAreTheSame(IMemberMapperData mapperData, Member sourceMember)
            => mapperData.TargetMember.LeafMember.Equals(sourceMember);

        private static bool MembersMatch(IMemberMapperData mapperData, Member sourceMember)
        {
            if (MembersAreTheSame(mapperData, sourceMember))
            {
                return true;
            }

            return mapperData
                .SourceMember
                .Append(sourceMember)
                .Matches(mapperData.TargetMember);
        }

        private static IEnumerable<IQualifiedMember> QuerySourceMembers(
            IQualifiedMember parentMember,
            IMemberMapperData mapperData,
            Func<IMemberMapperData, Member, bool> filter)
        {
            var members = GlobalContext
                .Instance
                .MemberCache
                .GetSourceMembers(parentMember.Type)
                .Filter(m => filter.Invoke(mapperData, m));

            if (!mapperData.RuleSet.Settings.AllowGetMethods)
            {
                members = members.Filter(m => m.MemberType != MemberType.GetMethod);
            }

            var qualifiedMembers = members.Project(parentMember.Append);

            if (mapperData.MapperContext.UserConfigurations.HasSourceMemberIgnores(mapperData))
            {
                qualifiedMembers = qualifiedMembers
                    .Filter(sm => IsNotUnconditionallyIgnored(sm, mapperData));
            }

            return qualifiedMembers;
        }

        private static bool IsNotUnconditionallyIgnored(IQualifiedMember sourceMember, IMemberMapperData mapperData)
        {
            var matchingIgnore = mapperData
                .MapperContext
                .UserConfigurations
                .GetSourceMemberIgnoreOrNull(new BasicMapperData(
                    sourceMember,
                    mapperData.TargetMember,
                    mapperData));

            return matchingIgnore?.HasConfiguredCondition != false;
        }

        private static bool TryFindParentContextSourceMemberMatch(
            IChildMemberMappingData targetMappingData,
            ref IQualifiedMember matchingMember,
            out SourceMemberMatch sourceMemberMatch)
        {
            var targetMapperData = targetMappingData.MapperData;
            var targetMember = targetMapperData.TargetMember;
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
                    targetMapperData,
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
                targetMappingData.MapperData,
                parentSourceMember,
                targetMappingData,
                ref matchingMember,
                out sourceMemberMatch);
        }

        private static bool TryFindSourceMemberMatch(
            IMemberMapperData targetMapperData,
            IQualifiedMember parentSourceMember,
            IChildMemberMappingData contextMappingData,
            ref IQualifiedMember matchingMember,
            out SourceMemberMatch sourceMemberMatch)
        {
            var candidateSourceMembers = EnumerateSourceMembers(parentSourceMember, contextMappingData)
                .Where(targetMapperData.TargetMember.Matches);

            foreach (var sourceMember in candidateSourceMembers)
            {
                if (TypesAreCompatible(sourceMember.Type, targetMapperData))
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

            var relevantChildSourceMembers = QuerySourceMembers(
                parentMember,
                rootData.MapperData,
                MembersHaveCompatibleTypes);

            foreach (var childMember in relevantChildSourceMembers)
            {
                if (childMember.IsSimple)
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

        private static bool MembersHaveCompatibleTypes(IMemberMapperData rootMapperData, Member sourceMember)
        {
            if (!sourceMember.IsSimple)
            {
                return true;
            }

            return rootMapperData.TargetMember.IsSimple ||
                   rootMapperData.TargetMember.Type == typeof(object);
        }

        private static bool TypesAreCompatible(Type sourceType, IMemberMapperData mapperData)
            => mapperData.CanConvert(sourceType, mapperData.TargetMember.Type);
    }
}