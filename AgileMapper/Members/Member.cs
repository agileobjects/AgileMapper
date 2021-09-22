namespace AgileObjects.AgileMapper.Members
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using AgileMapper.Extensions;
    using AgileMapper.Extensions.Internal;
    using Dictionaries;
    using Extensions;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;

    internal class Member
    {
        public const string RootSourceMemberName = "source";
        public const string RootTargetMemberName = "target";

        private readonly IAccessFactory _accessFactory;
        private readonly int _hashCode;

        private Member(
            MemberType memberType,
            Type type,
            MemberInfo memberInfo,
            IAccessFactory accessFactory = null,
            bool isReadable = true,
            bool isWriteable = true,
            bool isRoot = false)
            : this(
                  memberType,
                  memberInfo.Name,
                  memberInfo.DeclaringType,
                  type,
                  accessFactory,
                  isReadable,
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
            IAccessFactory accessFactory = null,
            bool isReadable = true,
            bool isWriteable = true,
            bool isRoot = false)
        {
            MemberType = memberType;
            Name = name;
            DeclaringType = declaringType;
            Type = type;
            _accessFactory = accessFactory;
            IsReadable = isReadable;
            IsWriteable = isWriteable;
            IsRoot = isRoot;

            _hashCode = declaringType.GetHashCode();

            unchecked
            {
                _hashCode = (_hashCode * 397) ^ name.GetHashCode();
            }

            JoiningName = (isRoot || this.IsEnumerableElement()) ? name : "." + name;
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
                MemberType.Parameter,
                name,
                typeof(IMappingExecutionContext),
                type,
                isRoot: true);
        }

        public static Member ConstructorParameter(ParameterInfo parameter)
        {
            return new Member(
                MemberType.ConstructorParameter,
                parameter.Name,
                parameter.Member.DeclaringType,
                parameter.ParameterType,
                isReadable: false);
        }

        public static Member Field(FieldInfo fieldInfo)
        {
            return new Member(
                MemberType.Field,
                fieldInfo.FieldType,
                fieldInfo,
                new FieldInfoWrapper(fieldInfo),
                isReadable: true,
                isWriteable: !fieldInfo.IsInitOnly);
        }

        public static Member Property(PropertyInfo propertyInfo)
        {
            return new Member(
                MemberType.Property,
                propertyInfo.PropertyType,
                propertyInfo,
                new PropertyInfoWrapper(propertyInfo),
                propertyInfo.IsReadable(),
                propertyInfo.IsWritable());
        }

        public static Member GetMethod(MethodInfo methodInfo)
        {
            return new Member(
                MemberType.GetMethod,
                methodInfo.ReturnType,
                methodInfo,
                new MethodInfoWrapper(methodInfo));
        }

        public static Member SetMethod(MethodInfo methodInfo)
        {
            return new Member(
                MemberType.SetMethod,
                methodInfo.GetParameters()[0].ParameterType,
                methodInfo,
                isReadable: false);
        }

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
            if (!IsReadable || (_accessFactory == null))
            {
                return Type.ToDefaultExpression();
            }

            if (!instance.Type.IsAssignableTo(DeclaringType))
            {
                instance = Expression.Convert(instance, DeclaringType);
            }

            return _accessFactory.GetAccess(instance);
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
                    IsReadable,
                    IsWriteable,
                    IsRoot);
            }

            return new Member(
                MemberType,
                Name,
                DeclaringType,
                runtimeType,
                isReadable: IsReadable,
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

        public bool Equals(Member otherMember) => otherMember?._hashCode == _hashCode;

        public override int GetHashCode() => _hashCode;

        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public override string ToString() => $"{Name}: {Type.GetFriendlyName()}";

        #region AccessFactories

        private interface IAccessFactory
        {
            Expression GetAccess(Expression instance);
        }

        private class FieldInfoWrapper : IAccessFactory
        {
            private readonly FieldInfo _fieldInfo;

            public FieldInfoWrapper(FieldInfo fieldInfo) => _fieldInfo = fieldInfo;

            public Expression GetAccess(Expression instance) => Expression.Field(instance, _fieldInfo);
        }

        private class PropertyInfoWrapper : IAccessFactory
        {
            private readonly PropertyInfo _propertyInfo;

            public PropertyInfoWrapper(PropertyInfo propertyInfo) => _propertyInfo = propertyInfo;

            public Expression GetAccess(Expression instance) => Expression.Property(instance, _propertyInfo);
        }

        private class MethodInfoWrapper : IAccessFactory
        {
            private readonly MethodInfo _methodInfo;

            public MethodInfoWrapper(MethodInfo methodInfo) => _methodInfo = methodInfo;

            public Expression GetAccess(Expression instance) => Expression.Call(instance, _methodInfo);
        }

        #endregion
    }
}