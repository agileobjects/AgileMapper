namespace AgileObjects.AgileMapper.Members
{
    using System;
    using Extensions;
    using ObjectPopulation;

    internal class Member
    {
        public Member(MemberType memberType, string name, Type declaringType, Type type)
        {
            MemberType = memberType;
            Name = name;
            DeclaringType = declaringType;
            Type = type;

            IsEnumerable = type.IsEnumerable();
            IsComplex = !IsEnumerable && type.IsComplex();

            if (IsEnumerable)
            {
                ElementType = Type.GetEnumerableElementType();
            }
        }

        #region Factory Methods

        public static Member RootSource(Type sourceType)
        {
            return Root("Source", sourceType);
        }

        public static Member RootTarget(Type type)
        {
            return Root("Target", type);
        }

        private static Member Root(string name, Type type)
        {
            return new Member(MemberType.Property, name, typeof(IObjectMappingContext), type);
        }

        #endregion

        public string Name { get; }

        public Type DeclaringType { get; }

        public Type Type { get; }

        public bool IsComplex { get; }

        public bool IsEnumerable { get; }

        public bool IsSimple => !(IsComplex || IsEnumerable);

        public Type ElementType { get; }

        public MemberType MemberType { get; }
    }
}