namespace AgileObjects.AgileMapper
{
    using System;
#if !NET_STANDARD
    using System.Diagnostics.CodeAnalysis;
#endif

    internal class SourceAndTargetTypesKey
    {
        public SourceAndTargetTypesKey(Type sourceType, Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
        }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public override bool Equals(object obj)
        {
            var otherKey = (SourceAndTargetTypesKey)obj;

            // ReSharper disable once PossibleNullReferenceException
            return (otherKey.SourceType == SourceType) &&
                   (otherKey.TargetType == TargetType);
        }

        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override int GetHashCode() => 0;
    }
}