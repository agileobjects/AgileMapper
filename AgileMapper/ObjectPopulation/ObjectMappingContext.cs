namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using DataSources;
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

        private static readonly ParameterExpression _instanceVariable = Expression.Variable(
            typeof(TInstance).IsEnumerable() ? EnumerableTypes.GetEnumerableVariableType<TInstance>() : typeof(TInstance),
            "instance");

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

        private static readonly MethodInfo _mapObjectMethod =
            _parameter.Type
                .GetMethods(Constants.PublicInstance)
                .First(m => (m.Name == "Map") && (m.GetParameters().Length == 4));

        private static readonly MethodInfo _mapEnumerableElementMethod =
            _parameter.Type
                .GetMethods(Constants.PublicInstance)
                .First(m => (m.Name == "Map") && (m.GetParameters().First().Name == "sourceElement"));
        // ReSharper restore StaticMemberInGenericType

        #endregion

        private readonly IQualifiedMember _sourceMember;
        private readonly IQualifiedMember _targetMember;
        private readonly int _sourceObjectDepth;

        public ObjectMappingContext(
            TRuntimeSource source,
            IQualifiedMember sourceMember,
            TRuntimeTarget target,
            IQualifiedMember targetMember,
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

        public TResult Try<TResult>(Func<TResult> funcToTry)
        {
            try
            {
                return funcToTry.Invoke();
            }
            catch
            {
                return default(TResult);
            }
        }

        public TDeclaredMember Map<TDeclaredSource, TDeclaredMember>(
            TDeclaredSource source,
            TDeclaredMember targetMemberValue,
            string targetMemberName,
            int dataSourceIndex)
        {
            var allTargetMembers = GlobalContext.MemberFinder.GetTargetMembers(_targetMember.Type);
            var targetMember = allTargetMembers.First(tm => tm.Name == targetMemberName);
            var qualifiedMember = _targetMember.Append(targetMember);
            var context = new MemberMappingContext(qualifiedMember, this);
            var sourceMember = context.GetDataSources().ElementAt(dataSourceIndex).SourceMember;

            var complexTargetMappingRequest = new ObjectMappingRequest<TDeclaredSource, TRuntimeTarget, TDeclaredMember>(
                source,
                sourceMember,
                Target,
                _targetMember,
                targetMemberValue,
                context.TargetMember,
                null,
                MappingContext);

            return MappingContext.MapChild(complexTargetMappingRequest);
        }

        public TTargetElement Map<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement existingElement,
            int enumerableIndex)
        {
            var sourceElementMember = _sourceMember.Append(_sourceMember.Type.CreateElementMember());
            var targetElementMember = _targetMember.Append(_targetMember.Type.CreateElementMember());

            var complexTargetMappingRequest = new ObjectMappingRequest<TSourceElement, TTargetElement, TTargetElement>(
                sourceElement,
                sourceElementMember,
                existingElement,
                targetElementMember,
                existingElement,
                targetElementMember,
                enumerableIndex,
                MappingContext);

            return MappingContext.MapChild(complexTargetMappingRequest);
        }

        #region IMemberMappingContext Members

        ParameterExpression IMemberMappingContext.Parameter => _parameter;

        string IMemberMappingContext.RuleSetName => MappingContext.RuleSet.Name;

        IQualifiedMember IMemberMappingContext.SourceMember => _sourceMember;

        Expression IMemberMappingContext.SourceObject => _sourceObjectProperty;

        int IMemberMappingContext.SourceObjectDepth => _sourceObjectDepth;

        Expression IMemberMappingContext.ExistingObject => _existingObjectProperty;

        Expression IMemberMappingContext.EnumerableIndex => _enumerableIndexProperty;

        ParameterExpression IMemberMappingContext.InstanceVariable => _instanceVariable;

        IQualifiedMember IMemberMappingContext.TargetMember => _targetMember;

        NestedAccessFinder IMemberMappingContext.NestedAccessFinder => _nestedAccessFinder;

        IEnumerable<IDataSource> IMemberMappingContext.GetDataSources() => this.GetDataSources();

        Expression IMemberMappingContext.GetTryCall(Expression expression)
        {
            var tryMethod = _parameter.Type
                .GetMethod("Try")
                .MakeGenericMethod(expression.Type);

            var tryArgument = Expression.Lambda(Expression.GetFuncType(expression.Type), expression);
            var tryCall = Expression.Call(_parameter, tryMethod, tryArgument);

            return tryCall;
        }

        #endregion

        #region IObjectMappingContext Members

        bool IObjectMappingContext.HasSource<TSource>(TSource source)
        {
            return ReferenceEquals(Source, source);
        }

        T IObjectMappingContext.GetInstance<T>() => (T)((object)CreatedInstance ?? Target);

        public int? GetEnumerableIndex() => EnumerableIndex.HasValue ? EnumerableIndex : Parent?.GetEnumerableIndex();

        Type IObjectMappingContext.GetSourceMemberRuntimeType(IQualifiedMember sourceMember)
        {
            if (sourceMember.Name == "Source")
            {
                // The root member is guaranteed to be the runtime type:
                return typeof(TRuntimeSource);
            }

            var relativeMember = sourceMember.RelativeTo(_sourceObjectDepth);
            var memberAccess = relativeMember.GetQualifiedAccess(_sourceObjectProperty);

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

        MethodCallExpression IObjectMappingContext.TryGetCall => _tryGetCall;

        MethodCallExpression IObjectMappingContext.CreateCall => _createCall;

        MethodCallExpression IObjectMappingContext.ObjectRegistrationCall => _registrationCall;

        MethodCallExpression IObjectMappingContext.GetMapCall(Expression sourceObject, IQualifiedMember objectMember, int dataSourceIndex)
        {
            var mapCall = Expression.Call(
                _parameter,
                _mapObjectMethod.MakeGenericMethod(sourceObject.Type, objectMember.Type),
                sourceObject,
                objectMember.GetAccess(_instanceVariable),
                Expression.Constant(objectMember.Name),
                Expression.Constant(dataSourceIndex));

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