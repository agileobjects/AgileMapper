namespace AgileObjects.AgileMapper.Members
{
    using System;
    using Extensions;
    using NetStandardPolyfills;

    internal class MappingTypes
    {
        private MappingTypes(
            Type sourceType,
            Type targetType,
            bool runtimeTypesNeeded,
            bool runtimeTypesAreTheSame,
            bool isEnumerable)
        {
            SourceType = sourceType;
            TargetType = targetType;
            RuntimeTypesNeeded = runtimeTypesNeeded;
            RuntimeTypesAreTheSame = runtimeTypesAreTheSame;
            IsEnumerable = isEnumerable;
        }

        #region Factory Method

        public static MappingTypes Fixed<TSource, TTarget>() => MappingTypesCache<TSource, TTarget>.Instance;

        public static MappingTypes For<TSource, TTarget>(TSource source, TTarget target)
        {
            var runtimeSourceTypeNeeded = TypeInfo<TSource>.RuntimeTypeNeeded;
            var runtimeTargetTypeNeeded = TypeInfo<TTarget>.RuntimeTypeNeeded;

            Type sourceType, targetType;
            bool runtimeTypesAreTheSame;

            if (runtimeSourceTypeNeeded)
            {
                sourceType = source.GetRuntimeSourceType();
                runtimeTypesAreTheSame = sourceType == typeof(TSource);

                if (!runtimeTargetTypeNeeded && !runtimeTypesAreTheSame &&
                    sourceType.IsDerivedFrom(typeof(TTarget)))
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
                }
            }
            else
            {
                targetType = typeof(TTarget);
            }

            var runtimeTypesNeeded = runtimeSourceTypeNeeded || runtimeTargetTypeNeeded;
            var isEnumerable = TypeInfo<TTarget>.IsEnumerable || (!runtimeTypesAreTheSame && targetType.IsEnumerable());

            return new MappingTypes(
                sourceType,
                targetType,
                runtimeTypesNeeded,
                runtimeTypesAreTheSame,
                isEnumerable);
        }

        #endregion

        public Type SourceType { get; }

        public Type TargetType { get; }

        public bool RuntimeTypesNeeded { get; }

        public bool RuntimeTypesAreTheSame { get; }

        public bool IsEnumerable { get; }

        public bool Equals(MappingTypes otherTypes)
            => (otherTypes.SourceType == SourceType) && (otherTypes.TargetType == TargetType);

        public MappingTypes WithTypes<TNewSource, TNewTarget>()
        {
            return new MappingTypes(
                typeof(TNewSource),
                typeof(TNewTarget),
                RuntimeTypesNeeded,
                RuntimeTypesAreTheSame,
                IsEnumerable);
        }

        private static class MappingTypesCache<TSource, TTarget>
        {
            public static readonly MappingTypes Instance = new MappingTypes(
                typeof(TSource),
                typeof(TTarget),
                runtimeTypesNeeded: false,
                runtimeTypesAreTheSame: true,
                isEnumerable: TypeInfo<TTarget>.IsEnumerable);
        }
    }
}