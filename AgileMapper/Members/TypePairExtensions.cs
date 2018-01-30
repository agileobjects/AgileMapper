namespace AgileObjects.AgileMapper.Members
{
    using System;
    using NetStandardPolyfills;

    internal static class TypePairExtensions
    {
        public static bool IsForAllSourceTypes(this ITypePair typePair)
            => typePair.SourceType == Constants.AllTypes;

        public static bool IsForSourceType(this ITypePair typePair, ITypePair otherTypePair)
            => typePair.IsForSourceType(otherTypePair.SourceType);

        private static bool IsForSourceType(this ITypePair typePair, Type sourceType)
            => IsForAllSourceTypes(typePair) || sourceType.IsAssignableTo(typePair.SourceType);

        public static bool IsForTargetType(this ITypePair typePair, ITypePair otherTypePair)
            => otherTypePair.TargetType.IsAssignableTo(typePair.TargetType);

        public static bool HasCompatibleTypes<TOtherTypePair>(
            this ITypePair typePair,
            TOtherTypePair otherTypePair,
            IQualifiedMember sourceMember = null,
            QualifiedMember targetMember = null)
            where TOtherTypePair : ITypePair
        {
            var sourceTypesMatch =
                typePair.IsForSourceType(otherTypePair.SourceType) ||
               (sourceMember?.HasCompatibleType(typePair.SourceType) == true);

            if (!sourceTypesMatch)
            {
                return false;
            }

            var targetTypesMatch =
               (targetMember?.HasCompatibleType(typePair.TargetType) == true) ||
                otherTypePair.TargetType.IsAssignableTo(typePair.TargetType);

            return targetTypesMatch;
        }
    }
}