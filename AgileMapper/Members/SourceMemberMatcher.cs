namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.Extensions;
    using TypeConversion;

    internal static class SourceMemberMatcher
    {
        public static SourceMemberMatch GetMatchFor(
            IChildMemberMappingData targetMappingData,
            bool searchParentContexts = true)
        {
            return GetMatchFor(new SourceMemberMatchContext(targetMappingData, searchParentContexts));
        }

        public static SourceMemberMatch GetMatchFor(SourceMemberMatchContext context)
        {
            if (context.ParentSourceMember.IsSimple)
            {
                return SourceMemberMatch.Null;
            }

            if (ExactMemberMatchExists(context))
            {
                return context.CreateSourceMemberMatch();
            }

            if (TryFindSourceMemberMatch(context) || TryFindParentContextSourceMemberMatch(context))
            {
                return context.SourceMemberMatch;
            }

            return (context.MatchingSourceMember != null)
                ? context.CreateSourceMemberMatch(isUseable: false)
                : SourceMemberMatch.Null;
        }

        private static bool ExactMemberMatchExists(SourceMemberMatchContext context)
        {
            var filter = context.TypesAreCompatible
                ? (Func<IMemberMapperData, Member, bool>)MembersAreTheSame
                : MembersMatch;

            var matchingSourceMember = QuerySourceMembers(context, filter).FirstOrDefault();

            if (matchingSourceMember == null)
            {
                return false;
            }

            context.MatchingSourceMember = matchingSourceMember;

            return TypesAreCompatible(matchingSourceMember.Type, context.MemberMapperData);
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
            SourceMemberMatchContext context,
            Func<IMemberMapperData, Member, bool> filter)
        {
            IEnumerable<Member> members = GlobalContext.Instance
                .MemberCache
                .GetSourceMembers(context.ParentSourceMember.Type);

            if (!context.MemberMapperData.RuleSet.Settings.AllowGetMethods)
            {
                members = members.Filter(m => m.MemberType != MemberType.GetMethod);
            }

            var qualifiedMembers = members
                .Filter(context.MemberMapperData, filter.Invoke)
                .Project(context.ParentSourceMember.Append);

            if (context.HasSourceMemberIgnores)
            {
                qualifiedMembers = qualifiedMembers.Filter(context, IsNotUnconditionallyIgnored);
            }

            return qualifiedMembers;
        }

        private static bool IsNotUnconditionallyIgnored(SourceMemberMatchContext context, IQualifiedMember sourceMember)
        {
            var matchingIgnore = context.GetSourceMemberIgnoreOrNull(sourceMember);

            return matchingIgnore?.HasConfiguredCondition != false;
        }

        private static bool TryFindParentContextSourceMemberMatch(SourceMemberMatchContext context)
        {
            if (!context.SearchParentContexts)
            {
                return false;
            }

            var mappingData = context.MemberMappingData.Parent;

            while (mappingData.Parent != null)
            {
                if (mappingData.MapperData.IsEntryPoint ||
                    mappingData.MapperData.TargetMemberIsEnumerableElement())
                {
                    break;
                }

                mappingData = mappingData.Parent;

                var childMapperData = new ChildMemberMapperData(context.TargetMember, mappingData.MapperData);
                var contextMappingData = mappingData.GetChildMappingData(childMapperData);

                if (TryFindSourceMemberMatch(context.With(contextMappingData)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryFindSourceMemberMatch(SourceMemberMatchContext context)
        {
            var candidateSourceMembers = EnumerateSourceMembers(context)
                .Filter(context.TargetMember.Matches);

            foreach (var sourceMember in candidateSourceMembers)
            {
                if (TypesAreCompatible(sourceMember.Type, context.MemberMapperData))
                {
                    context.SourceMemberMatch = context.CreateSourceMemberMatch(sourceMember);
                    return true;
                }

                if (context.MatchingSourceMember == null)
                {
                    context.MatchingSourceMember = sourceMember;
                }
            }

            return false;
        }

        private static IEnumerable<IQualifiedMember> EnumerateSourceMembers(SourceMemberMatchContext context)
        {
            yield return context.ParentSourceMember;

            if (!context.ParentSourceMember.CouldMatch(context.TargetMember))
            {
                yield break;
            }

            var parentMemberType = context.MemberMappingData.Parent
                .GetSourceMemberRuntimeType(context.ParentSourceMember);

            if (parentMemberType != context.ParentSourceMember.Type)
            {
                context.With(context.ParentSourceMember.WithType(parentMemberType));
                yield return context.ParentSourceMember;

                if (context.ParentSourceMember.IsSimple)
                {
                    yield break;
                }
            }

            var relevantChildSourceMembers = QuerySourceMembers(context, MembersHaveCompatibleTypes);

            foreach (var childMember in relevantChildSourceMembers)
            {
                if (childMember.IsSimple)
                {
                    yield return childMember;
                    continue;
                }

                foreach (var qualifiedMember in EnumerateSourceMembers(context.With(childMember)))
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