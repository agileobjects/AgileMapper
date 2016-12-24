namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using Extensions;
    using ObjectPopulation;

    [DebuggerDisplay("{Name}: {Type.Name}")]
    internal class Member
    {
        public Member(
            MemberType memberType,
            string name,
            Type declaringType,
            Type type,
            bool isWriteable = true,
            bool isRoot = false)
        {
            MemberType = memberType;
            Name = name;
            JoiningName = (isRoot || this.IsEnumerableElement()) ? name : "." + name;
            IsRoot = isRoot;
            IsIdentifier = IsIdMember(name, declaringType);
            DeclaringType = declaringType;
            Type = type;

            IsReadable = memberType.IsReadable();
            IsWriteable = isWriteable;

            IsEnumerable = type.IsEnumerable();

            if (IsEnumerable)
            {
                ElementType = Type.GetEnumerableElementType();
                return;
            }

            IsSimple = type.IsSimple();
            IsComplex = !IsSimple;
        }

        #region Setup

        private static bool IsIdMember(string name, Type declaringType)
        {
            return (name == "Id") ||
                   (name == declaringType.Name + "Id") ||
                   (name == "Identifier") ||
                   (name == declaringType.Name + "Identifier");
        }

        #endregion

        #region Factory Methods

        public static Member RootSource<TSource>() => SourceMemberCache<TSource>.MemberInstance;

        public static Member RootSource(Type type) => RootSource("Source", type);

        public static Member RootSource(string signature, Type type) => Root(signature, type);

        public static Member RootTarget<TTarget>() => TargetMemberCache<TTarget>.MemberInstance;

        public static Member RootTarget(Type type) => Root("Target", type);

        private static Member Root(string name, Type type)
        {
            return new Member(
                MemberType.Property,
                name,
                typeof(ObjectMapperData),
                type,
                isRoot: true);
        }

        public static Member ConstructorParameter(ParameterInfo parameter)
            => new Member(MemberType.ConstructorParameter, parameter.Name, parameter.Member.DeclaringType, parameter.ParameterType);

        public static Member Field(FieldInfo field)
            => new Member(MemberType.Field, field.Name, field.DeclaringType, field.FieldType, !field.IsInitOnly);

        public static Member Property(PropertyInfo property)
            => new Member(MemberType.Property, property.Name, property.DeclaringType, property.PropertyType, property.IsWriteable());

        public static Member GetMethod(MethodInfo method)
            => new Member(MemberType.GetMethod, method.Name, method.DeclaringType, method.ReturnType);

        public static Member SetMethod(MethodInfo method)
            => new Member(MemberType.SetMethod, method.Name, method.DeclaringType, method.GetParameters()[0].ParameterType);

        public static Member EnumerableElement(Type enumerableType, Type elementType = null)
        {
            return new Member(
                MemberType.EnumerableElement,
                Constants.EnumerableElementName,
                enumerableType,
                elementType ?? enumerableType.GetEnumerableElementType());
        }

        public static Member DictionaryEntry(Member sourceMember, DictionaryTargetMember targetMember)
            => new Member(MemberType.DictionaryEntry, sourceMember.Name, targetMember.Type, targetMember.ValueType);

        #endregion

        public string Name { get; }

        public string JoiningName { get; }

        public Type DeclaringType { get; }

        public Type Type { get; }

        public bool IsRoot { get; }

        public bool IsIdentifier { get; }

        public bool IsComplex { get; }

        public bool IsEnumerable { get; }

        public bool IsSimple { get; }

        public bool IsReadable { get; }

        public bool IsWriteable { get; }

        public Type ElementType { get; }

        public MemberType MemberType { get; }

        public Member WithType(Type runtimeType)
        {
            return (runtimeType == Type)
                ? this
                : new Member(MemberType, Name, DeclaringType, runtimeType);
        }

        private static class SourceMemberCache<T>
        {
            public static readonly Member MemberInstance = RootSource(typeof(T));
        }

        private static class TargetMemberCache<T>
        {
            public static readonly Member MemberInstance = RootTarget(typeof(T));
        }
    }
}