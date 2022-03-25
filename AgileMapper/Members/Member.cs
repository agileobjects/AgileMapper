namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Reflection;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Dictionaries;
    using Extensions;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;

    internal class Member
    {
        public const string RootSourceMemberName = "Source";
        public const string RootTargetMemberName = "Target";

        private static readonly int _ctorParameterHashCode =
            MemberType.ConstructorParameter.GetHashCode();

        private readonly IAccessFactory _accessFactory;
        private readonly int _hashCode;

        private Member(
            MemberType memberType,
            Type type,
            MemberInfo memberInfo,
            IAccessFactory accessFactory = null,
            bool isIndexed = false,
            bool isReadable = true,
            bool isWriteable = true,
            bool isRoot = false)
            : this(
                  memberType,
                  memberInfo.Name,
                  memberInfo.DeclaringType,
                  type,
                  accessFactory,
                  isIndexed,
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
            bool isIndexed = false,
            bool isReadable = true,
            bool isWriteable = true,
            bool isRoot = false)
        {
            MemberType = memberType;
            Name = name;
            DeclaringType = declaringType;
            Type = type;
            _accessFactory = accessFactory;
            IsIndexed = isIndexed;
            IsReadable = isReadable;
            IsWriteable = isWriteable;
            IsRoot = isRoot;

            _hashCode = declaringType.GetHashCode();

            unchecked
            {
                _hashCode = (_hashCode * 397) ^ name.GetHashCode();

                if (memberType == MemberType.ConstructorParameter)
                {
                    _hashCode = (_hashCode * 397) ^ _ctorParameterHashCode;
                }
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
            var isReadable = propertyInfo.IsReadable();
            var isWritable = propertyInfo.IsWritable();

            var isIndexed =
                isReadable && propertyInfo.GetGetter().GetParameters().Any() ||
                isWritable && propertyInfo.GetSetter().GetParameters().Length > 1;

            return new Member(
                MemberType.Property,
                propertyInfo.PropertyType,
                propertyInfo,
                new PropertyInfoWrapper(propertyInfo),
                isIndexed,
                isReadable,
                isWritable);
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

        public bool IsIndexed { get; set; }

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
                    IsIndexed,
                    IsReadable,
                    IsWriteable,
                    IsRoot);
            }

            return new Member(
                MemberType,
                Name,
                DeclaringType,
                runtimeType,
                isIndexed: IsIndexed,
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