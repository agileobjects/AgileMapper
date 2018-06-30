namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions.Internal;

    internal static class SourceMemberMatcher
    {
        public static IQualifiedMember GetMatchFor(
            IChildMemberMappingData targetData,
            out IChildMemberMappingData contextMappingData,
            bool searchParentContexts = true)
        {
            var parentSourceMember = targetData.MapperData.SourceMember;

            if (parentSourceMember.IsSimple)
            {
                contextMappingData = null;
                return null;
            }

            if (ExactMatchingSourceMemberExists(parentSourceMember, targetData, out var matchingMember))
            {
                contextMappingData = targetData;
                return GetFinalSourceMember(matchingMember, targetData);
            }

            matchingMember = EnumerateSourceMembers(parentSourceMember, targetData)
                .FirstOrDefault(sm => IsMatchingMember(sm, targetData.MapperData));

            if (matchingMember != null)
            {
                contextMappingData = targetData;
                return GetFinalSourceMember(matchingMember, targetData);
            }

            if (searchParentContexts)
            {
                return GetParentContextMatchOrNull(targetData, out contextMappingData);
            }

            contextMappingData = null;
            return null;
        }

        private static IQualifiedMember GetParentContextMatchOrNull(
            IChildMemberMappingData targetData,
            out IChildMemberMappingData contextMappingData)
        {
            var mappingData = targetData.Parent;

            while (mappingData.Parent != null)
            {
                if (mappingData.MapperData.IsEntryPoint ||
                    mappingData.MapperData.TargetMemberIsEnumerableElement())
                {
                    break;
                }

                mappingData = mappingData.Parent;

                var childMapperData = new ChildMemberMapperData(targetData.MapperData.TargetMember, mappingData.MapperData);
                contextMappingData = mappingData.GetChildMappingData(childMapperData);

                var matchingMember = EnumerateSourceMembers(mappingData.MapperData.SourceMember, contextMappingData)
                    .FirstOrDefault(sm => IsMatchingMember(sm, targetData.MapperData));

                if (matchingMember != null)
                {
                    return GetFinalSourceMember(matchingMember, targetData);
                }
            }

            contextMappingData = null;
            return null;
        }

        private static bool ExactMatchingSourceMemberExists(
            IQualifiedMember parentSourceMember,
            IChildMemberMappingData targetData,
            out IQualifiedMember matchingMember)
        {
            var sourceMember = QuerySourceMembers(
                parentSourceMember,
                targetData,
                MembersMatch)
                .FirstOrDefault();

            if ((sourceMember == null) || !TypesAreCompatible(sourceMember.Type, targetData.MapperData))
            {
                matchingMember = null;
                return false;
            }

            matchingMember = parentSourceMember.Append(sourceMember);
            return true;
        }

        private static bool MembersMatch(IChildMemberMappingData mappingData, Member sourceMember)
        {
            if (mappingData.MapperData.TargetMember.LeafMember.Equals(sourceMember))
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

        private static IQualifiedMember GetFinalSourceMember(
            IQualifiedMember sourceMember,
            IChildMemberMappingData targetData)
        {
            return targetData
                .MapperData
                .MapperContext
                .QualifiedMemberFactory
                .GetFinalSourceMember(sourceMember, targetData.MapperData.TargetMember);
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

        private static bool IsMatchingMember(IQualifiedMember sourceMember, IMemberMapperData mapperData)
            => mapperData.TargetMember.Matches(sourceMember) && TypesAreCompatible(sourceMember.Type, mapperData);

        private static bool TypesAreCompatible(Type sourceType, IMemberMapperData mapperData)
            => mapperData.CanConvert(sourceType, mapperData.TargetMember.Type);
    }
}