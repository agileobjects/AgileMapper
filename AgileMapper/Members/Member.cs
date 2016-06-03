namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Extensions;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;

    internal class Member
    {
        public Member(
            MemberType memberType,
            string name,
            Type declaringType,
            Type type,
            bool isRoot = false)
        {
            MemberType = memberType;
            Name = name;
            MemberName = new MemberName(name, declaringType, memberType, isRoot);
            DeclaringType = declaringType;
            Type = type;
            Signature = $"[{declaringType.GetFriendlyName()}].{name}";

            IsEnumerable = type.IsEnumerable();
            IsComplex = !IsEnumerable && type.IsComplex();

            if (IsEnumerable)
            {
                ElementType = Type.GetEnumerableElementType();
            }
        }

        #region Factory Methods

        public static Member RootSource(Type sourceType) => Root("Source", sourceType);

        public static Member ConfiguredSource(string signature, Type type) => Root(signature, type);

        public static Member RootTarget(Type type) => Root("Target", type);

        private static Member Root(string name, Type type)
        {
            return new Member(
                MemberType.Property,
                name,
                typeof(IObjectMappingContext),
                type,
                isRoot: true);
        }

        public static Member ConstructorParameter(ParameterInfo parameter)
            => new Member(MemberType.ConstructorParameter, parameter.Name, parameter.Member.DeclaringType, parameter.ParameterType);

        public static Member Field(FieldInfo field)
            => new Member(MemberType.Field, field.Name, field.DeclaringType, field.FieldType);

        public static Member Property(PropertyInfo property)
            => new Member(MemberType.Property, property.Name, property.DeclaringType, property.PropertyType);

        public static Member GetMethod(MethodInfo method)
            => new Member(MemberType.GetMethod, method.Name, method.DeclaringType, method.ReturnType);

        public static Member SetMethod(MethodInfo method)
            => new Member(MemberType.SetMethod, method.Name, method.DeclaringType, method.GetParameters().First().ParameterType);

        #endregion

        public string Name { get; }

        public MemberName MemberName { get; }

        public Type DeclaringType { get; }

        public Type Type { get; }

        public string Signature { get; }

        public bool IsComplex { get; }

        public bool IsEnumerable { get; }

        public bool IsSimple => !(IsComplex || IsEnumerable);

        public bool IsIdentifier => MemberName.IsIdentifier;

        public Type ElementType { get; }

        public MemberType MemberType { get; }

        public bool IsReadable => MemberType.IsReadable();

        public Member WithType(Type runtimeType)
        {
            return (runtimeType == Type)
                ? this
                : new Member(MemberType, Name, DeclaringType, runtimeType);
        }
    }
}