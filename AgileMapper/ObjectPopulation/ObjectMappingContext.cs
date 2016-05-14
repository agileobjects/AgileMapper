namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;

    internal class ObjectMappingContext<TRuntimeSource, TRuntimeTarget, TInstance> :
        InstanceCreationContext<TRuntimeSource, TRuntimeTarget, TInstance>,
        IObjectMappingContext
    {
        #region Cached Items

        private static readonly ParameterExpression _parameter =
            Parameters.Create<ObjectMappingContext<TRuntimeSource, TRuntimeTarget, TInstance>>("omc");

        // ReSharper disable StaticMemberInGenericType
        private static readonly Expression _sourceObjectProperty = Expression.Property(_parameter, "Source");

        private static readonly Expression _existingObjectProperty = Expression.Property(_parameter, "ExistingInstance");

        private static readonly Expression _enumerableIndexProperty = Expression.Property(_parameter, "EnumerableIndex");

        private static readonly ParameterExpression _instanceVariable =
            Expression.Variable(typeof(TInstance).GetInstanceVariableType(), "instance");

        private static readonly NestedAccessFinder _nestedAccessFinder = new NestedAccessFinder(_parameter);

        private static readonly Expression _mappingContextProperty = Expression.Property(_parameter, "MappingContext");

        private static readonly MethodCallExpression _tryGetCall = Expression.Call(
            _mappingContextProperty,
                _mappingContextProperty.Type
                    .GetMethod("TryGet", Constants.PublicInstance)
                    .MakeGenericMethod(_sourceObjectProperty.Type, _instanceVariable.Type),
                _sourceObjectProperty,
                _instanceVariable);

        private static readonly MethodCallExpression _createCall = Expression.Call(
            _parameter,
            _parameter.Type.GetMethod("Create", Constants.PublicInstance));

        private static readonly MethodCallExpression _registrationCall = Expression.Call(
            _mappingContextProperty,
                _mappingContextProperty.Type
                    .GetMethod("Register", Constants.PublicInstance)
                    .MakeGenericMethod(_sourceObjectProperty.Type, _instanceVariable.Type),
                _sourceObjectProperty,
                _instanceVariable);

        private static readonly MethodInfo _mapComplexTypeMethod =
            _parameter.Type
                .GetMethods(Constants.PublicInstance)
                .First(m => (m.Name == "Map") && m.GetParameters().Length == 1);

        private static readonly MethodInfo _mapEnumerableMethod =
            _parameter.Type
                .GetMethods(Constants.PublicInstance)
                .First(m => (m.Name == "Map") && (m.GetParameters().First().Name == "sourceEnumerable"));

        private static readonly MethodInfo _mapEnumerableElementMethod =
            _parameter.Type
                .GetMethods(Constants.PublicInstance)
                .First(m => (m.Name == "Map") && (m.GetParameters().First().Name == "sourceElement"));
        // ReSharper restore StaticMemberInGenericType

        #endregion

        private readonly QualifiedMember _sourceMember;
        private readonly QualifiedMember _targetMember;
        private readonly int _sourceObjectDepth;

        public ObjectMappingContext(
            QualifiedMember sourceMember,
            QualifiedMember targetMember,
            TRuntimeSource source,
            TRuntimeTarget target,
            TInstance existingInstance,
            int? enumerableIndex,
            MappingContext mappingContext)
            : base(source, target, existingInstance, enumerableIndex)
        {
            _sourceMember = sourceMember;
            _targetMember = targetMember;
            MappingContext = mappingContext;
            Parent = mappingContext.CurrentObjectMappingContext;
            _sourceObjectDepth = CalculateSourceObjectDepth();
        }

        private int CalculateSourceObjectDepth()
        {
            var parent = Parent;

            while (parent != null)
            {
                if (parent.HasSource(Source))
                {
                    parent = parent.Parent;
                    continue;
                }

                return parent.SourceObjectDepth + 1;
            }

            return 0;
        }

        public GlobalContext GlobalContext => MapperContext.GlobalContext;

        public MapperContext MapperContext => MappingContext.MapperContext;

        public MappingContext MappingContext { get; }

        public IObjectMappingContext Parent { get; }

        public TInstance Create() => (CreatedInstance = MapperContext.ComplexTypeFactory.Create<TInstance>());

        public TMember Map<TMember>(Expression<Func<TInstance, TMember>> complexChildMember)
        {
            TMember existingInstance;
            Type targetMemberRuntimeType;

            if (ExistingInstance != null)
            {
                existingInstance = complexChildMember.Compile().Invoke(ExistingInstance);
                targetMemberRuntimeType = existingInstance.GetRuntimeTargetType(typeof(TRuntimeSource));
            }
            else
            {
                existingInstance = default(TMember);
                targetMemberRuntimeType = typeof(TMember);
            }

            var childTargetMember = GetChildTargetMember(complexChildMember, targetMemberRuntimeType);

            var qualifiedChildTargetMember = _targetMember.Append(childTargetMember);

            return MappingContext.MapChild(Source, Target, qualifiedChildTargetMember, existingInstance);
        }

        public TDeclaredMember Map<TDeclaredSource, TDeclaredMember>(
            TDeclaredSource sourceEnumerable,
            TDeclaredMember targetEnumerable,
            Expression<Func<TInstance, TDeclaredMember>> targetEnumerableMember)
        {
            var runtimeSourceType = sourceEnumerable.GetRuntimeSourceType();
            var targetMemberRuntimeType = targetEnumerable.GetRuntimeTargetType(runtimeSourceType);
            var childTargetMember = GetChildTargetMember(targetEnumerableMember, targetMemberRuntimeType);

            var qualifiedChildTargetMember = _targetMember.Append(childTargetMember);

            return MappingContext.MapChild(sourceEnumerable, targetEnumerable, qualifiedChildTargetMember, targetEnumerable);
        }

        private static Member GetChildTargetMember<TDeclaredMember>(
            Expression<Func<TInstance, TDeclaredMember>> childTargetMemberExpression,
            Type targetMemberRuntimeType)
        {
            var childTargetMemberInfo = ((MemberExpression)childTargetMemberExpression.Body).Member;
            var childTargetMemberType = (childTargetMemberInfo is PropertyInfo) ? MemberType.Property : MemberType.Field;

            return new Member(
                childTargetMemberType,
                childTargetMemberInfo.Name,
                typeof(TRuntimeTarget),
                targetMemberRuntimeType);
        }

        public TTargetElement Map<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement existingElement,
            int enumerableIndex)
        {
            return MappingContext.MapEnumerableElement(sourceElement, existingElement, enumerableIndex);
        }

        #region IMemberMappingContext Members

        ParameterExpression IMemberMappingContext.Parameter => _parameter;

        string IMemberMappingContext.RuleSetName => MappingContext.RuleSet.Name;

        Expression IMemberMappingContext.SourceObject => _sourceObjectProperty;

        int IMemberMappingContext.SourceObjectDepth => _sourceObjectDepth;

        Expression IMemberMappingContext.ExistingObject => _existingObjectProperty;

        Expression IMemberMappingContext.EnumerableIndex => _enumerableIndexProperty;

        ParameterExpression IMemberMappingContext.InstanceVariable => _instanceVariable;

        QualifiedMember IMemberMappingContext.TargetMember => _targetMember;

        NestedAccessFinder IMemberMappingContext.NestedAccessFinder => _nestedAccessFinder;

        #endregion

        #region IObjectMappingContext Members

        bool IObjectMappingContext.HasSource<TSource>(TSource source)
        {
            return ReferenceEquals(Source, source);
        }

        T IObjectMappingContext.GetInstance<T>() => (T)(object)CreatedInstance;

        public int? GetEnumerableIndex() => EnumerableIndex.HasValue ? EnumerableIndex : Parent?.GetEnumerableIndex();

        Type IObjectMappingContext.GetSourceMemberRuntimeType(QualifiedMember sourceMember)
        {
            if (sourceMember.Members.Count() == 1)
            {
                // The root member is guaranteed to be the runtime type:
                return typeof(TRuntimeSource);
            }

            var relativeMember = sourceMember.RelativeTo(_sourceObjectDepth);
            var memberAccess = relativeMember.GetAccess(_sourceObjectProperty);

            var getRuntimeTypeCall = Expression.Call(
                typeof(ObjectExtensions)
                    .GetMethod("GetRuntimeSourceType", Constants.PublicStatic)
                    .MakeGenericMethod(sourceMember.Type),
                memberAccess);

            var getRuntimeTypeLambda = Expression
                .Lambda<Func<ObjectMappingContext<TRuntimeSource, TRuntimeTarget, TInstance>, Type>>(
                    getRuntimeTypeCall,
                    _parameter);

            var getRuntimeTypeFunc = getRuntimeTypeLambda.Compile();

            return getRuntimeTypeFunc.Invoke(this);
        }

        QualifiedMember IObjectMappingContext.SourceMember => _sourceMember;

        MethodCallExpression IObjectMappingContext.GetTryGetCall() => _tryGetCall;

        MethodCallExpression IObjectMappingContext.GetCreateCall() => _createCall;

        MethodCallExpression IObjectMappingContext.GetObjectRegistrationCall() => _registrationCall;

        MethodCallExpression IObjectMappingContext.GetMapCall(Member complexTypeMember)
        {
            var mapCall = Expression.Call(
                _parameter,
                _mapComplexTypeMethod.MakeGenericMethod(complexTypeMember.Type),
                GetTargetMemberLambda(complexTypeMember));

            return mapCall;
        }

        private static LambdaExpression GetTargetMemberLambda(Member objectMember)
        {
            var targetObjectParameter = Parameters.Create<TInstance>("o");
            var targetMemberAccess = objectMember.GetAccess(targetObjectParameter);

            var targetMemberLambda = Expression.Lambda(
                Expression.GetFuncType(targetObjectParameter.Type, objectMember.Type),
                targetMemberAccess,
                targetObjectParameter);

            return targetMemberLambda;
        }

        MethodCallExpression IObjectMappingContext.GetMapCall(Expression sourceEnumerable, Member enumerableMember)
        {
            var typedMapMethod = _mapEnumerableMethod
                .MakeGenericMethod(sourceEnumerable.Type, enumerableMember.Type);

            var mapCall = Expression.Call(
                _parameter,
                typedMapMethod,
                sourceEnumerable,
                enumerableMember.GetAccess(_instanceVariable),
                GetTargetMemberLambda(enumerableMember));

            return mapCall;
        }

        MethodCallExpression IObjectMappingContext.GetMapCall(Expression sourceElement, Expression existingElement)
        {
            var typedMapMethod = _mapEnumerableElementMethod
                .MakeGenericMethod(sourceElement.Type, existingElement.Type);

            var mapCall = Expression.Call(
                _parameter,
                typedMapMethod,
                sourceElement,
                existingElement,
                Parameters.EnumerableIndex);

            return mapCall;
        }

        #endregion
    }
}