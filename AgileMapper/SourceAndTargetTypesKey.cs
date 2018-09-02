namespace AgileObjects.AgileMapper
{
    using System;

    internal class SourceAndTargetTypesKey
    {
        private readonly int _hashCode;

        public SourceAndTargetTypesKey(Type sourceType, Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;

            unchecked
            {
                _hashCode = (sourceType.GetHashCode() * 397) ^ targetType.GetHashCode();
            }
        }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public override bool Equals(object obj)
        {
            // ReSharper disable once PossibleNullReferenceException
            return obj.GetHashCode() == _hashCode;
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override int GetHashCode() => _hashCode;
    }
}