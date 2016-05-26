namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal class TypeIdentifierKey
    {
        private readonly string _value;

        private TypeIdentifierKey(string value)
        {
            _value = value;
        }

        public static TypeIdentifierKey For(Type type) => new TypeIdentifierKey(type.FullName + ": Id");

        public static implicit operator string(TypeIdentifierKey key) => key._value;
    }
}