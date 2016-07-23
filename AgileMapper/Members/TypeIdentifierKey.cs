namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal class TypeIdentifierKey
    {
        private readonly Type _type;

        public TypeIdentifierKey(Type type)
        {
            _type = type;
        }

        public override bool Equals(object obj)
        {
            var otherKey = obj as TypeIdentifierKey;

            if (otherKey == null)
            {
                return false;
            }

            return _type == otherKey._type;
        }

        public override int GetHashCode() => 0;
    }
}