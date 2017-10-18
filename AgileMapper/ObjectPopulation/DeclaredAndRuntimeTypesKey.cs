namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Members;

    internal class DeclaredAndRuntimeTypesKey
    {
        private readonly bool _sourceTypesAreTheSame;
        private readonly bool _targetTypesAreTheSame;

        private DeclaredAndRuntimeTypesKey(
            Type declaredSourceType,
            Type declaredTargetType,
            Type runtimeSourceType,
            Type runtimeTargetType)
        {
            DeclaredSourceType = declaredSourceType;
            RuntimeSourceType = runtimeSourceType;
            _sourceTypesAreTheSame = (declaredSourceType == runtimeSourceType);
            DeclaredTargetType = declaredTargetType;
            RuntimeTargetType = runtimeTargetType;
            _targetTypesAreTheSame = (declaredTargetType == runtimeTargetType);
        }

        public static DeclaredAndRuntimeTypesKey For<TSource, TTarget>(MappingTypes mappingTypes)
        {
            return new DeclaredAndRuntimeTypesKey(
                typeof(TSource),
                typeof(TTarget),
                mappingTypes.SourceType,
                mappingTypes.TargetType);
        }

        public Type DeclaredSourceType { get; }

        public Type DeclaredTargetType { get; }

        public Type RuntimeSourceType { get; }

        public Type RuntimeTargetType { get; }

        public override bool Equals(object obj)
        {
            var otherKey = (DeclaredAndRuntimeTypesKey)obj;

            return
                // ReSharper disable once PossibleNullReferenceException
                (DeclaredSourceType == otherKey.DeclaredSourceType) &&
                ((_sourceTypesAreTheSame && otherKey._sourceTypesAreTheSame) || RuntimeSourceType == otherKey.RuntimeSourceType) &&
                (DeclaredTargetType == otherKey.DeclaredTargetType) &&
                ((_targetTypesAreTheSame && otherKey._targetTypesAreTheSame) || RuntimeTargetType == otherKey.RuntimeTargetType);
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override int GetHashCode() => 0;
    }
}