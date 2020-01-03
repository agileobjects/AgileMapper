namespace AgileObjects.AgileMapper.Members
{
    using NetStandardPolyfills;

    internal static class TypePairExtensions
    {
        public static bool IsForAllSourceTypes(this ITypePair typePair)
            => typePair.SourceType == Constants.AllTypes;

        public static bool IsForSourceType(this ITypePair typePair, ITypePair otherTypePair)
            => IsForAllSourceTypes(typePair) || otherTypePair.SourceType.IsAssignableTo(typePair.SourceType);

        public static bool IsForTargetType(this ITypePair typePair, ITypePair otherTypePair)
            => otherTypePair.TargetType.IsAssignableTo(typePair.TargetType);

        public static bool HasTypesCompatibleWith(this ITypePair typePair, ITypePair otherTypePair) 
            => typePair.IsForSourceType(otherTypePair) && typePair.IsForTargetType(otherTypePair);
    }
}