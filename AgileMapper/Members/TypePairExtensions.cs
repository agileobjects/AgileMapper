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

        public static bool HasCompatibleTypes(
            this ITypePair typePair,
            ITypePair otherTypePair,
            Func<bool> sourceTypeMatcher = null,
            Func<bool> targetTypeMatcher = null)
        {
            var sourceTypesMatch =
                typePair.IsForSourceType(otherTypePair.SourceType) ||
               (sourceTypeMatcher?.Invoke() == true);

            if (!sourceTypesMatch)
            {
                return false;
            }

            var targetTypesMatch =
                targetTypeMatcher?.Invoke() ??
                otherTypePair.TargetType.IsAssignableTo(typePair.TargetType);

            return targetTypesMatch;
        }
    }
}