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

        public override int GetHashCode() => _hashCode;
    }
}