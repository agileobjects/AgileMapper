namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        private static readonly int _ctorParameterHashCode =
            MemberType.ConstructorParameter.GetHashCode();

        private readonly IAccessFactory _accessFactory;
        private readonly int _hashCode;

        private Member(
            MemberType memberType,
            Type type,
            MemberInfo memberInfo,
            IAccessFactory accessFactory = null,
            IList<ParameterInfo> requiredIndexes = null,
            bool isReadable = true,
            bool isWriteable = true,
            bool isRoot = false)
            : this(
                  memberType,
                  memberInfo.Name,
                  memberInfo.DeclaringType,
                  type,
                  accessFactory,
                  requiredIndexes,
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
            IList<ParameterInfo> requiredIndexes = null,
            bool isReadable = true,
            bool isWriteable = true,
            bool isRoot = false)
        {
            MemberType = memberType;
            Name = name;
            DeclaringType = declaringType;
            Type = type;
            _accessFactory = accessFactory;
            RequiredIndexes = requiredIndexes ?? Enumerable<ParameterInfo>.EmptyArray;
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
            var propertyInfoWrapper = PropertyInfoWrapper.For(propertyInfo);

            return new Member(
                MemberType.Property,
                propertyInfo.PropertyType,
                propertyInfo,
                propertyInfoWrapper,
                propertyInfoWrapper.RequiredIndexes,
                propertyInfoWrapper.IsReadable,
                propertyInfoWrapper.IsWritable);
        }

        public static Member GetMethod(MethodInfo methodInfo)
        {
            return new Member(
                MemberType.GetMethod,
                methodInfo.ReturnType,
                methodInfo,
                new MethodInfoWrapper(methodInfo),
                isWriteable: false);
        }

        public static Member SetMethod(MethodInfo methodInfo)
        {
            return new Member(
                MemberType.SetMethod,
                methodInfo.GetParameters()[0].ParameterType,
                methodInfo,
                new MethodInfoWrapper(methodInfo),
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

        public IList<ParameterInfo> RequiredIndexes { get; }

        public bool IsReadable { get; }

        public bool IsWriteable { get; }

        public bool HasMatchingCtorParameter { get; set; }

        public Type ElementType { get; }

        public MemberType MemberType { get; }

        public bool HasAttribute<TAttribute>()
            => MemberInfo?.HasAttribute<TAttribute>() == true;

        public Expression GetReadAccess(Expression instance)
        {
            if (!IsReadable || (_accessFactory == null))
            {
                return Type.ToDefaultExpression();
            }

            if (!instance.Type.IsAssignableTo(DeclaringType))
            {
                instance = Expression.Convert(instance, DeclaringType);
            }

            return _accessFactory.GetReadAccess(instance);
        }

        public Expression GetAssignment(Expression instance, Expression value)
        {
            if (!IsWriteable || (_accessFactory == null))
            {
                return Type.ToDefaultExpression();
            }

            if (!instance.Type.IsAssignableTo(DeclaringType))
            {
                instance = Expression.Convert(instance, DeclaringType);
            }

            return _accessFactory.GetAssignment(instance, value);
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
                    RequiredIndexes,
                    IsReadable,
                    IsWriteable,
                    IsRoot);
            }

            return new Member(
                MemberType,
                Name,
                DeclaringType,
                runtimeType,
                requiredIndexes: RequiredIndexes,
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
            Expression GetReadAccess(Expression instance);

            Expression GetAssignment(Expression instance, Expression value);
        }

        private class FieldInfoWrapper : IAccessFactory
        {
            private readonly FieldInfo _fieldInfo;

            public FieldInfoWrapper(FieldInfo fieldInfo) => _fieldInfo = fieldInfo;

            public Expression GetReadAccess(Expression instance)
                => Expression.Field(instance, _fieldInfo);

            public Expression GetAssignment(Expression instance, Expression value)
                => GetReadAccess(instance).AssignTo(value);
        }

        private interface IPropertyAccessFactory : IAccessFactory
        {
            bool IsReadable { get; }

            bool IsWritable { get; }

            IList<ParameterInfo> RequiredIndexes { get; }
        }

        private static class PropertyInfoWrapper
        {
            public static IPropertyAccessFactory For(PropertyInfo propertyInfo)
            {
                var getter = propertyInfo.GetGetter();
                var getterParameters = getter?.GetParameters();

                var setter = propertyInfo.GetSetter();
                IList<ParameterInfo> setterParameters;

                if (setter != null)
                {
                    setterParameters = setter.GetParameters();
                    var parameterCount = setterParameters.Count;

                    if (parameterCount > 1)
                    {
                        var setterIndexes = new ParameterInfo[parameterCount - 1];
                        setterIndexes.CopyFrom(setterParameters);
                        setterParameters = setterIndexes;
                    }
                    else
                    {
                        setterParameters = Enumerable<ParameterInfo>.EmptyArray;
                    }
                }
                else
                {
                    setterParameters = Enumerable<ParameterInfo>.EmptyArray;
                }

                if (getterParameters.NoneOrNull() && setterParameters.None())
                {
                    return new StandardPropertyInfoWrapper(propertyInfo);
                }

                var arguments = (getterParameters ?? setterParameters)
                    .ProjectToArray<ParameterInfo, Expression>(p =>
                    {
                        var defaultValue = p.DefaultValue;

                        if (defaultValue == null || 
#if FEATURE_DBNULL
                            defaultValue == DBNull.Value)
#else
                            defaultValue.GetType().Name == "DBNull")
#endif
                        {
                            return p.ParameterType.ToDefaultExpression();
                        }

                        if (p.ParameterType.IsValueType())
                        {
                            defaultValue = Convert.ChangeType(defaultValue, p.ParameterType);
                        }

                        return Expression.Constant(defaultValue, p.ParameterType);
                    });


                return new IndexedPropertyInfoWrapper(
                    propertyInfo,
                    getter,
                    getterParameters ?? Enumerable<ParameterInfo>.EmptyArray,
                    setter,
                    setterParameters,
                    arguments);
            }

            private class StandardPropertyInfoWrapper : IPropertyAccessFactory
            {
                private readonly PropertyInfo _propertyInfo;

                public StandardPropertyInfoWrapper(PropertyInfo propertyInfo)
                {
                    _propertyInfo = propertyInfo;
                    IsReadable = propertyInfo.IsReadable();
                    IsWritable = propertyInfo.IsWritable();
                }

                public bool IsReadable { get; }

                public bool IsWritable { get; }

                public IList<ParameterInfo> RequiredIndexes
                    => Enumerable<ParameterInfo>.EmptyArray;

                public Expression GetReadAccess(Expression instance)
                    => Expression.Property(instance, _propertyInfo);

                public Expression GetAssignment(Expression instance, Expression value)
                    => GetReadAccess(instance).AssignTo(value);
            }

            private class IndexedPropertyInfoWrapper : IPropertyAccessFactory
            {
                private readonly PropertyInfo _propertyInfo;
                private readonly MethodInfo _setter;
                private readonly Expression[] _arguments;

                public IndexedPropertyInfoWrapper(
                    PropertyInfo propertyInfo,
                    MethodInfo getter,
                    IEnumerable<ParameterInfo> getterParameters,
                    MethodInfo setter,
                    IEnumerable<ParameterInfo> setterParameters,
                    Expression[] arguments)
                {
                    _propertyInfo = propertyInfo;
                    _setter = setter;
                    _arguments = arguments;
                    IsReadable = getter != null;
                    IsWritable = setter != null;

                    RequiredIndexes = IsReadable
                        ? getterParameters.QueryRequired().ToList()
                        : setterParameters.QueryRequired().ToList();
                }

                public bool IsReadable { get; }

                public bool IsWritable { get; }

                public IList<ParameterInfo> RequiredIndexes { get; }

                public Expression GetReadAccess(Expression instance)
                    => Expression.Property(instance, _propertyInfo, _arguments);

                public Expression GetAssignment(Expression instance, Expression value)
                    => Expression.Call(instance, _setter, _arguments.Append(value));
            }
        }

        private class MethodInfoWrapper : IAccessFactory
        {
            private readonly MethodInfo _methodInfo;

            public MethodInfoWrapper(MethodInfo methodInfo)
            {
                _methodInfo = methodInfo;
            }

            public Expression GetReadAccess(Expression instance)
                => Expression.Call(instance, _methodInfo);

            public Expression GetAssignment(Expression instance, Expression value)
                => Expression.Call(instance, _methodInfo, value);
        }
        #endregion
    }
}