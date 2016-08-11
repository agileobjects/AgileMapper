namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Members;

    internal class DeclaredAndRuntimeTypesKey
    {
        private readonly KeyType _keyType;
        private readonly Type _declaredSourceType;
        private readonly Type _runtimeSourceType;
        private readonly Type _declaredTargetType;
        private readonly Type _runtimeTargetType;
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
            _declaredSourceType = declaredSourceType;
            _runtimeSourceType = runtimeSourceType;
            _sourceTypesAreTheSame = (declaredSourceType == runtimeSourceType);
            _declaredTargetType = declaredTargetType;
            _runtimeTargetType = runtimeTargetType;
            _targetTypesAreTheSame = (declaredTargetType == runtimeTargetType);
        }

        public static DeclaredAndRuntimeTypesKey From(
            ObjectMapperDataFactoryBridge command)
        {
            return new DeclaredAndRuntimeTypesKey(
                KeyType.OmcConstructor,
                command.DeclaredSourceType,
                command.DeclaredTargetType,
                command.SourceMember.Type,
                command.TargetMember.Type);
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

        public override bool Equals(object obj)
        {
            var otherKey = (DeclaredAndRuntimeTypesKey)obj;

            return
                (_keyType == otherKey._keyType) &&
                (_declaredSourceType == otherKey._declaredSourceType) &&
                ((_sourceTypesAreTheSame && otherKey._sourceTypesAreTheSame) || _runtimeSourceType == otherKey._runtimeSourceType) &&
                (_declaredTargetType == otherKey._declaredTargetType) &&
                ((_targetTypesAreTheSame && otherKey._targetTypesAreTheSame) || _runtimeTargetType == otherKey._runtimeTargetType);
        }

        public override int GetHashCode() => 0;

        private enum KeyType { OmcConstructor, CreateMapperCall }
    }
}