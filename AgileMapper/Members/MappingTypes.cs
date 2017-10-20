namespace AgileObjects.AgileMapper.Members
{
    using System;
    using Extensions;
    using NetStandardPolyfills;

    internal class MappingTypes
    {
        public MappingTypes(
            Type sourceType,
            Type targetType,
            bool runtimeTypesAreTheSame,
            bool isEnumerable)
        {
            SourceType = sourceType;
            TargetType = targetType;
            RuntimeTypesAreTheSame = runtimeTypesAreTheSame;
            IsEnumerable = isEnumerable;
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

            if (!runtimeTypesNeeded)
            {
                return MappingTypes<TSource, TTarget>.Fixed;
            }

            var isEnumerable = TypeInfo<TTarget>.IsEnumerable ||
                ((targetType != typeof(TTarget)) && targetType.IsEnumerable());

            return new MappingTypes(
                sourceType,
                targetType,
                false, // <- runtimeTypesAreTheSame
                isEnumerable);
        }

        #endregion

        public Type SourceType { get; }

        public Type TargetType { get; }

        public bool RuntimeTypesNeeded => !RuntimeTypesAreTheSame;

        public bool RuntimeTypesAreTheSame { get; }

        public bool IsEnumerable { get; }

        public bool Equals(MappingTypes otherTypes)
        {
            if (otherTypes == this)
            {
                return true;
            }

            return (otherTypes.SourceType == SourceType) && (otherTypes.TargetType == TargetType);
        }

        public MappingTypes WithTypes<TNewSource, TNewTarget>()
        {
            return new MappingTypes(
                typeof(TNewSource),
                typeof(TNewTarget),
                RuntimeTypesAreTheSame,
                IsEnumerable);
        }
    }

    internal static class MappingTypes<TSource, TTarget>
    {
        public static readonly bool SkipTypesCheck =
            !(TypeInfo<TSource>.RuntimeTypeNeeded || TypeInfo<TTarget>.RuntimeTypeNeeded);

        public static readonly MappingTypes Fixed = new MappingTypes(
            typeof(TSource),
            typeof(TTarget),
            true, // <- runtimeTypesAreTheSame
            TypeInfo<TTarget>.IsEnumerable);
    }
}