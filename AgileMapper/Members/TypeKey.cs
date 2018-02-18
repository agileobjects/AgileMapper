namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal class TypeKey
    {
        private readonly int _hashCode;

        private TypeKey(Type type, KeyType keyType, string name = null)
        {
            Type = type;
            Name = name;

            unchecked
            {
                _hashCode = ((int)keyType * 397) ^ type.GetHashCode();
            }
        }

        public static TypeKey ForSourceMembers(Type type) => new TypeKey(type, KeyType.SourceMembers);

        public static TypeKey ForTargetMembers(Type type) => new TypeKey(type, KeyType.TargetMembers);

        public static TypeKey ForTypeId(Type type) => new TypeKey(type, KeyType.TypeId);

        public static TypeKey ForParameter(Type type, string name) => new TypeKey(type, KeyType.Parameter, name);

        public Type Type { get; }

        public string Name { get; }

        public override bool Equals(object obj)
        {
            // ReSharper disable once PossibleNullReferenceException
            return obj.GetHashCode() == _hashCode;
        }

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override int GetHashCode() => _hashCode;

        private enum KeyType { SourceMembers, TargetMembers, TypeId, Parameter }
    }
}