namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;

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

        public override int GetHashCode() => 0;
    }
}