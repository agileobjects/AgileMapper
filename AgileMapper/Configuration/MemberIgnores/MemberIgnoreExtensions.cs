namespace AgileObjects.AgileMapper.Configuration.MemberIgnores
{
    internal static class MemberIgnoreExtensions
    {
        public static string GetConflictMessage(
            this IMemberFilterIgnore memberFilterIgnore,
            IMemberIgnoreBase conflictingMemberIgnore)
        {
            if (conflictingMemberIgnore is IMemberIgnore otherMemberIgnore)
            {
                return memberFilterIgnore.GetConflictMessage(otherMemberIgnore);
            }

            var otherIgnoredMemberFilter = (IMemberFilterIgnore)conflictingMemberIgnore;

            return $"Ignore pattern '{otherIgnoredMemberFilter.MemberFilter}' has already been configured";
        }

        public static string GetConflictMessage(
            this IMemberFilterIgnore memberFilterIgnore,
            IMemberIgnore conflictingMemberIgnore)
        {
            return $"Member {conflictingMemberIgnore.Member.GetPath()} is " +
                   $"already ignored by ignore pattern '{memberFilterIgnore.MemberFilter}'";
        }

        public static string GetConflictMessage(
            this IMemberIgnore memberIgnore,
            IMemberIgnoreBase conflictingMemberIgnore)
        {
            if (conflictingMemberIgnore is IMemberFilterIgnore memberFilterIgnore)
            {
                return memberFilterIgnore.GetConflictMessage(memberIgnore);
            }

            return $"Member {memberIgnore.Member.GetPath()} has already been ignored";
        }
    }
}