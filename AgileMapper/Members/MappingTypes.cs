namespace AgileObjects.AgileMapper.Members
{
    using System;
    using Extensions;

    internal class MappingTypes
    {
        public MappingTypes(
            Type sourceType,
            Type targetType,
            MapperContext mapperContext)
            : this(sourceType, targetType, true, targetType.IsEnumerable(), mapperContext)
        {
        }

        private MappingTypes(
            Type sourceType,
            Type targetType,
            bool runtimeTypesAreTheSame,
            bool isEnumerable,
            MapperContext mapperContext)
        {
            SourceType = sourceType;
            TargetType = targetType;
            RuntimeTypesAreTheSame = runtimeTypesAreTheSame;
            IsEnumerable = isEnumerable;
            MapperContext = mapperContext;
        }

        public static MappingTypes For<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            IMappingContext mappingContext,
            IBasicMappingData parent)
        {
            var sourceType = GetSourceType(source);
            var targetType = GetTargetType(source, target, enumerableIndex, sourceType, mappingContext, parent);
            var runtimeTypesAreTheSame = (sourceType == typeof(TSource)) && (targetType == typeof(TTarget));
            var isEnumerable = TypeInfo<TTarget>.IsEnumerable || targetType.IsEnumerable();

            return new MappingTypes(
                sourceType,
                targetType,
                runtimeTypesAreTheSame,
                isEnumerable,
                mappingContext.MapperContext);
        }

        private static Type GetSourceType<TSource>(TSource source)
            => TypeInfo<TSource>.CheckSourceType ? source.GetRuntimeSourceType() : typeof(TSource);

        private static Type GetTargetType<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            Type sourceType,
            IMappingContext mappingContext,
            IBasicMappingData parent)
        {
            if (!TypeInfo<TTarget>.CheckTargetType)
            {
                return typeof(TTarget);
            }

            var targetMemberType = mappingContext.MapperContext.UserConfigurations.DerivedTypePairs
                .GetDerivedTypeOrNull(
                    source,
                    target,
                    enumerableIndex,
                    sourceType,
                    mappingContext,
                    parent) ?? target.GetRuntimeTargetType(sourceType);

            return targetMemberType;
        }

        public Type SourceType { get; }

        public Type TargetType { get; }

        public bool RuntimeTypesAreTheSame { get; }

        public bool IsEnumerable { get; }

        public MapperContext MapperContext { get; }

        public override bool Equals(object obj)
        {
            var otherTypes = (MappingTypes)obj;

            // ReSharper disable once PossibleNullReferenceException
            return (otherTypes.SourceType == SourceType) && (otherTypes.TargetType == TargetType);
        }

        public override int GetHashCode() => 0;
    }
}