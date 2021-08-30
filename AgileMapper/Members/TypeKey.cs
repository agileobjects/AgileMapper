namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal class TypeKey
    {
        private readonly int _hashCode;

        private TypeKey(Type type, KeyType keyType, string name = null)
        {
            Type = type;

            unchecked
            {
                _hashCode = ((int)keyType * 397) ^ type.GetHashCode();

                if (name == null)
                {
                    return;
                }

                Name = name;
                _hashCode = (_hashCode * 397) ^ name.GetHashCode();
            }
        }

        public static TypeKey ForSourceMembers(Type type) => new(type, KeyType.SourceMembers);

        public static TypeKey ForTargetMembers(Type type) => new(type, KeyType.TargetMembers);

        public static TypeKey ForTypeId(Type type) => new(type, KeyType.TypeId);

        public static TypeKey ForParameter(Type type, string name) => new(type, KeyType.Parameter, name);

        public Type Type { get; }

        public string Name { get; }

        public override int GetHashCode() => _hashCode;

        private enum KeyType { SourceMembers, TargetMembers, TypeId, Parameter }
    }
}