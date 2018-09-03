namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;

    internal class DeclaredAndRuntimeTypesKey
    {
        private readonly bool _sourceTypesAreTheSame;
        private readonly bool _targetTypesAreTheSame;
        private readonly int _hashCode;

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

            _hashCode = DeclaredSourceType.GetHashCode();

            unchecked
            {
                _hashCode = (_hashCode * 397) ^ DeclaredTargetType.GetHashCode();
            }
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
            // ReSharper disable once PossibleNullReferenceException
            if (_hashCode != obj.GetHashCode())
            {
                return false;
            }

            var otherKey = (DeclaredAndRuntimeTypesKey)obj;

            return
                ((_sourceTypesAreTheSame && otherKey._sourceTypesAreTheSame) || RuntimeSourceType == otherKey.RuntimeSourceType) &&
                ((_targetTypesAreTheSame && otherKey._targetTypesAreTheSame) || RuntimeTargetType == otherKey.RuntimeTargetType);
        }

        public override int GetHashCode() => _hashCode;
    }
}