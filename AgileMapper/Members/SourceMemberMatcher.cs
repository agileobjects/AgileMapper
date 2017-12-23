namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class SourceMemberMatcher
    {
        public static IQualifiedMember GetMatchFor(
            IChildMemberMappingData targetData,
            out IChildMemberMappingData contextMappingData)
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

            var mappingData = targetData.Parent;

            while (mappingData.Parent != null)
            {
                if (mappingData.MapperData.TargetMemberIsEnumerableElement())
                {
                    contextMappingData = null;
                    return null;
                }

                mappingData = mappingData.Parent;

                var childMapperData = new ChildMemberMapperData(targetData.MapperData.TargetMember, mappingData.MapperData);
                contextMappingData = mappingData.GetChildMappingData(childMapperData);

                matchingMember = EnumerateSourceMembers(mappingData.MapperData.SourceMember, contextMappingData)
                    .FirstOrDefault(sm => IsMatchingMember(sm, targetData.MapperData));

                if (matchingMember == null)
                {
                    continue;
                }

                return GetFinalSourceMember(matchingMember, targetData);
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
                m => targetData.MapperData.TargetMember.LeafMember.Equals(m) ||
                     targetData.MapperData.TargetMember.JoinedNames.Match(new[] { m.Name }))
                .FirstOrDefault();

            if ((sourceMember == null) ||
                !TypesAreCompatible(sourceMember.Type, targetData.MapperData))
            {
                matchingMember = null;
                return false;
            }

            matchingMember = parentSourceMember.Append(sourceMember);
            return true;
        }

        private static IEnumerable<Member> QuerySourceMembers(
            IQualifiedMember parentMember,
            Func<Member, bool> filter)
        {
            return GlobalContext
                .Instance
                .MemberCache
                .GetSourceMembers(parentMember.Type)
                .Where(filter);
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

            var parentMemberType = rootData.GetSourceMemberRuntimeType(parentMember);

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
                sourceMember => MembersHaveCompatibleTypes(sourceMember, rootData));

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
            return mapperData.TargetMember.Matches(sourceMember) && TypesAreCompatible(sourceMember.Type, mapperData);
        }

        private static bool TypesAreCompatible(Type sourceType, IMemberMapperData mapperData)
        {
            return mapperData.MapperContext.ValueConverters.CanConvert(sourceType, mapperData.TargetMember.Type);
        }
    }
}