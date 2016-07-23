namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal class TypeIdentifierKey
    {
        public TypeIdentifierKey(Type type)
        {
            Type = type;
        }

        public Type Type { get; }

        public override bool Equals(object obj)
        {
            var otherKey = obj as TypeIdentifierKey;

            if (otherKey == null)
            {
                return false;
            }

            return Type == otherKey.Type;
        }

        public override int GetHashCode() => 0;
    }
}