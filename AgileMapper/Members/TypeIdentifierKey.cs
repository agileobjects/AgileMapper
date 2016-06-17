namespace AgileObjects.AgileMapper.Members
{
    using System;

    internal class TypeIdentifierKey
    {
        private TypeIdentifierKey(string value)
        {
            Value = value;
        }

        public static TypeIdentifierKey For(Type type) => new TypeIdentifierKey(type.FullName + ": Id");

        public string Value { get; }
    }
}