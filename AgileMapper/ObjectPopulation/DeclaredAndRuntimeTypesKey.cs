namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Members;

    internal class DeclaredAndRuntimeTypesKey
    {
        private readonly KeyType _keyType;
        private readonly bool _sourceTypesAreTheSame;
        private readonly bool _targetTypesAreTheSame;

        private DeclaredAndRuntimeTypesKey(
            KeyType keyType,
            Type declaredSourceType,
            Type declaredTargetType,
            Type runtimeSourceType,
            Type runtimeTargetType)
        {
            _keyType = keyType;
            DeclaredSourceType = declaredSourceType;
            RuntimeSourceType = runtimeSourceType;
            _sourceTypesAreTheSame = (declaredSourceType == runtimeSourceType);
            DeclaredTargetType = declaredTargetType;
            RuntimeTargetType = runtimeTargetType;
            _targetTypesAreTheSame = (declaredTargetType == runtimeTargetType);
        }

        public static DeclaredAndRuntimeTypesKey ForMappingDataConstructor(
            Type declaredSourceType,
            Type declaredTargetType,
            Type runtimeSourceType,
            Type runtimeTargetType)
        {
            return new DeclaredAndRuntimeTypesKey(
                KeyType.MappingDataConstructor,
                declaredSourceType,
                declaredTargetType,
                runtimeSourceType,
                runtimeTargetType);
        }

        public static DeclaredAndRuntimeTypesKey ForCreateMapperCall<TSource, TTarget>(MemberMapperData data)
        {
            return new DeclaredAndRuntimeTypesKey(
                KeyType.CreateMapperCall,
                typeof(TSource),
                typeof(TTarget),
                data.SourceMember.Type,
                data.TargetMember.Type);
        }

        public Type DeclaredSourceType { get; }

        public Type DeclaredTargetType { get; }

        public Type RuntimeSourceType { get; }

        public Type RuntimeTargetType { get; }

        public override bool Equals(object obj)
        {
            var otherKey = (DeclaredAndRuntimeTypesKey)obj;

            return
                (_keyType == otherKey._keyType) &&
                (DeclaredSourceType == otherKey.DeclaredSourceType) &&
                ((_sourceTypesAreTheSame && otherKey._sourceTypesAreTheSame) || RuntimeSourceType == otherKey.RuntimeSourceType) &&
                (DeclaredTargetType == otherKey.DeclaredTargetType) &&
                ((_targetTypesAreTheSame && otherKey._targetTypesAreTheSame) || RuntimeTargetType == otherKey.RuntimeTargetType);
        }

        public override int GetHashCode() => 0;

        private enum KeyType { MappingDataConstructor, CreateMapperCall }
    }
}