namespace AgileObjects.AgileMapper
{
    using System;
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal class MappingTypes
    {
        private readonly int _hashCode;

        public MappingTypes(
            Type sourceType,
            Type targetType,
            bool runtimeTypesAreTheSame)
        {
            SourceType = sourceType;
            TargetType = targetType;
            RuntimeTypesAreTheSame = runtimeTypesAreTheSame;

            unchecked
            {
                _hashCode = (sourceType.GetHashCode() * 397) ^ targetType.GetHashCode();
            }
        }

        #region Factory Method

        public static MappingTypes For<TSource, TTarget>(TSource source, TTarget target)
        {
            if (MappingTypes<TSource, TTarget>.SkipTypesCheck)
            {
                return MappingTypes<TSource, TTarget>.Fixed;
            }

            var runtimeSourceTypeNeeded = TypeInfo<TSource>.RuntimeTypeNeeded;
            var runtimeTargetTypeNeeded = TypeInfo<TTarget>.RuntimeTypeNeeded;

            Type sourceType, targetType;
            bool runtimeTypesAreTheSame;

            if (runtimeSourceTypeNeeded)
            {
                sourceType = source.GetRuntimeSourceType();
                runtimeTypesAreTheSame = sourceType == typeof(TSource);

                if (runtimeTypesAreTheSame)
                {
                    runtimeSourceTypeNeeded = source == null;
                }
                else if (!runtimeTargetTypeNeeded && sourceType.IsDerivedFrom(typeof(TTarget)))
                {
                    runtimeTargetTypeNeeded = true;
                }
            }
            else
            {
                sourceType = typeof(TSource);
                runtimeTypesAreTheSame = true;
            }

            if (runtimeTargetTypeNeeded)
            {
                targetType = target.GetRuntimeTargetType(sourceType);

                if (runtimeTypesAreTheSame)
                {
                    runtimeTypesAreTheSame = targetType == typeof(TTarget);

                    if (runtimeTypesAreTheSame)
                    {
                        runtimeTargetTypeNeeded = target == null;
                    }
                }
            }
            else
            {
                targetType = typeof(TTarget);
            }

            var runtimeTypesNeeded = runtimeSourceTypeNeeded || runtimeTargetTypeNeeded;

            if (runtimeTypesNeeded)
            {
                return new MappingTypes(sourceType, targetType, runtimeTypesAreTheSame: false);
            }

            return MappingTypes<TSource, TTarget>.Fixed;
        }

        #endregion

        public Type SourceType { get; }

        public Type TargetType { get; }

        public bool RuntimeTypesNeeded => !RuntimeTypesAreTheSame;

        public bool RuntimeTypesAreTheSame { get; }

        public bool Equals(MappingTypes otherTypes) => otherTypes._hashCode == _hashCode;

        public override int GetHashCode() => _hashCode;

        public MappingTypes WithTypes<TNewSource, TNewTarget>()
        {
            if (RuntimeTypesAreTheSame)
            {
                return this;
            }

            return new MappingTypes(
                typeof(TNewSource),
                typeof(TNewTarget),
                RuntimeTypesAreTheSame);
        }
    }

    internal static class MappingTypes<TSource, TTarget>
    {
        public static readonly bool SkipTypesCheck =
            !(TypeInfo<TSource>.RuntimeTypeNeeded || TypeInfo<TTarget>.RuntimeTypeNeeded);

        public static readonly MappingTypes Fixed = new MappingTypes(
            typeof(TSource),
            typeof(TTarget),
            true);
    }
}