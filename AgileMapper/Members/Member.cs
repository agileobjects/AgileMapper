namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Reflection;
    using Dictionaries;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class Member
    {
        public const string RootSourceMemberName = "Source";
        public const string RootTargetMemberName = "Target";

        private readonly Func<Expression, MemberInfo, Expression> _accessFactory;
        private readonly int _hashCode;

        private Member(
            MemberType memberType,
            Type type,
            MemberInfo memberInfo,
            Func<Expression, MemberInfo, Expression> accessFactory = null,
            bool isWriteable = true,
            bool isRoot = false)
            : this(
                  memberType,
                  memberInfo.Name,
                  memberInfo.DeclaringType,
                  type,
                  accessFactory,
                  isWriteable,
                  isRoot)
        {
            MemberInfo = memberInfo;
        }

        private Member(
            MemberType memberType,
            string name,
            Type declaringType,
            Type type,
            Func<Expression, MemberInfo, Expression> accessFactory = null,
            bool isWriteable = true,
            bool isRoot = false)
        {
            MemberType = memberType;
            Name = name;
            DeclaringType = declaringType;
            Type = type;
            _accessFactory = accessFactory;
            IsWriteable = isWriteable;
            IsRoot = isRoot;

            _hashCode = declaringType.GetHashCode();

            unchecked
            {
                _hashCode = (_hashCode * 397) ^ name.GetHashCode();
            }

            JoiningName = (isRoot || this.IsEnumerableElement()) ? name : "." + name;
            IsReadable = memberType.IsReadable();
            IsEnumerable = type.IsEnumerable();

            if (IsEnumerable)
            {
                IsComplex = IsDictionary = type.IsDictionary();
                ElementType = Type.GetEnumerableElementType();
                return;
            }

            IsSimple = type.IsSimple();
            IsComplex = !IsSimple;
        }

        #region Factory Methods

        public static Member RootSource<TSource>() => SourceMemberCache<TSource>.MemberInstance;

        public static Member RootSource(Type type) => RootSource(RootSourceMemberName, type);

        public static Member RootSource(string signature, Type type) => Root(signature, type);

        public static Member RootTarget<TTarget>() => TargetMemberCache<TTarget>.MemberInstance;

        public static Member RootTarget(Type type) => Root(RootTargetMemberName, type);

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
        {
            return new Member(
                MemberType.ConstructorParameter,
                parameter.Name,
                parameter.Member.DeclaringType,
                parameter.ParameterType);
        }

        public static Member Field(FieldInfo field)
        {
            return new Member(
                MemberType.Field,
                field.FieldType,
                field,
                (instance, f) => Expression.Field(instance, (FieldInfo)f),
                !field.IsInitOnly);
        }

        public static Member Property(PropertyInfo property)
        {
            return new Member(
                MemberType.Property,
                property.PropertyType,
                property,
                (instance, p) => Expression.Property(instance, (PropertyInfo)p),
                property.IsWritable());
        }

        public static Member GetMethod(MethodInfo method)
        {
            return new Member(
                MemberType.GetMethod,
                method.ReturnType,
                method,
                (instance, m) => Expression.Call(instance, (MethodInfo)m));
        }

        public static Member SetMethod(MethodInfo method)
            => new Member(MemberType.SetMethod, method.GetParameters()[0].ParameterType, method);

        public static Member EnumerableElement(Type enumerableType, Type elementType = null)
        {
            return new Member(
                MemberType.EnumerableElement,
                Constants.EnumerableElementName,
                enumerableType,
                elementType ?? enumerableType.GetEnumerableElementType());
        }

        public static Member DictionaryEntry(string sourceMemberName, DictionaryTargetMember targetMember)
        {
            return new Member(
                MemberType.DictionaryEntry,
                sourceMemberName,
                targetMember.Type,
                targetMember.ValueType);
        }

        #endregion

        public MemberInfo MemberInfo { get; }

        public string Name { get; }

        public string JoiningName { get; }

        public Type DeclaringType { get; }

        public Type Type { get; }

        public bool IsRoot { get; }

        public bool IsComplex { get; }

        public bool IsEnumerable { get; }

        public bool IsDictionary { get; }

        public bool IsSimple { get; }

        public bool IsReadable { get; }

        public bool IsWriteable { get; }

        public bool HasMatchingCtorParameter { get; set; }

        public Type ElementType { get; }

        public MemberType MemberType { get; }

        public bool HasAttribute<TAttribute>()
            => MemberInfo?.HasAttribute<TAttribute>() == true;

        public Expression GetAccess(Expression instance)
        {
            if (!IsReadable)
            {
                return Type.ToDefaultExpression();
            }

            if (!instance.Type.IsAssignableTo(DeclaringType))
            {
                instance = Expression.Convert(instance, DeclaringType);
            }

            return _accessFactory.Invoke(instance, MemberInfo);
        }

        public Member WithType(Type runtimeType)
        {
            if (_accessFactory != null)
            {
                return new Member(
                    MemberType,
                    runtimeType,
                    MemberInfo,
                    _accessFactory,
                    IsWriteable,
                    IsRoot);
            }

            return new Member(
                MemberType,
                Name,
                DeclaringType,
                runtimeType,
                isWriteable: IsWriteable,
                isRoot: IsRoot);
        }

        private static class SourceMemberCache<T>
        {
            public static readonly Member MemberInstance = RootSource(typeof(T));
        }

        private static class TargetMemberCache<T>
        {
            public static readonly Member MemberInstance = RootTarget(typeof(T));
        }

        public bool Equals(Member otherMember) => otherMember._hashCode == _hashCode;

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString() => $"{Name}: {Type.GetFriendlyName()}";
    }
}