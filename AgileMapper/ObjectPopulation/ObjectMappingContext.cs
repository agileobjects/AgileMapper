namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Api.Configuration;
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

        private static readonly MethodInfo _tryActionMethod = GetTryMethod("action");
        private static readonly MethodInfo _tryUntypedHandlerActionMethod = GetTryMethod("action", typeof(IUntypedMemberMappingExceptionContext));
        private static readonly MethodInfo _tryHalfTypedHandlerActionMethod = GetTryMethod("action", typeof(ITypedMemberMappingExceptionContext<object, TRuntimeTarget>));
        private static readonly MethodInfo _tryTypedHandlerActionMethod = GetTryMethod("action", typeof(ITypedMemberMappingExceptionContext<TRuntimeSource, TRuntimeTarget>));

        private static readonly MethodInfo _tryFuncMethod = GetTryMethod("funcToTry");
        private static readonly MethodInfo _tryUntypedHandlerFuncMethod = GetTryMethod("funcToTry", typeof(IUntypedMemberMappingExceptionContext));
        private static readonly MethodInfo _tryHalfTypedHandlerFuncMethod = GetTryMethod("funcToTry", typeof(ITypedMemberMappingExceptionContext<object, TRuntimeTarget>));
        private static readonly MethodInfo _tryTypedHandlerFuncMethod = GetTryMethod("funcToTry", typeof(ITypedMemberMappingExceptionContext<TRuntimeSource, TRuntimeTarget>));

        private static MethodInfo GetTryMethod(string firstParameterName, Type callbackType = null)
        {
            if (callbackType != null)
            {
                callbackType = typeof(Action<>).MakeGenericType(callbackType);
            }

            return _parameter.Type
                .GetMethods(Constants.PublicInstance)
                .Select(m => new
                {
                    Method = m,
                    Parameters = m.GetParameters()
                })
                .First(d =>
                    (d.Method.Name == "Try") &&
                    (d.Parameters[0].Name == firstParameterName) &&
                    ((callbackType == null) ||
                    (d.Parameters.Length > 1 && (d.Parameters[1].ParameterType == callbackType))))
                .Method;
        }

        private static readonly MethodCallExpression _registrationCall = Expression.Call(
            _mappingContextProperty,
                _mappingContextProperty.Type
                    .GetMethod("Register", Constants.PublicInstance)
                    .MakeGenericMethod(_sourceObjectProperty.Type, _instanceVariable.Type),
                _sourceObjectProperty,
                _instanceVariable);

        private static readonly MethodInfo _mapObjectMethod = _parameter.Type
            .GetMethods(Constants.PublicInstance)
            .First(m => (m.Name == "Map") && (m.GetParameters().Length == 4));

        private static readonly MethodInfo _mapEnumerableElementMethod = _parameter.Type
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

        IMappingData IMappingData.Parent => Parent;

        IObjectMappingContext IMemberMappingContext.Parent => Parent;

        public IObjectMappingContext Parent { get; }

        public TInstance Create() => (CreatedInstance = MapperContext.ComplexTypeFactory.Create<TInstance>());

        #region Try

        public void Try(Action action)
            => Try(action, (ITypedMemberMappingExceptionContext<TRuntimeSource, TRuntimeTarget> ctx) => { });

        public void Try(Action action, Action<IUntypedMemberMappingExceptionContext> callback)
        {
            Try(action,
                (ITypedMemberMappingExceptionContext<TRuntimeSource, TRuntimeTarget> ctx)
                    => callback((IUntypedMemberMappingExceptionContext)ctx));
        }

        public void Try(
            Action action,
            Action<ITypedMemberMappingExceptionContext<object, TRuntimeTarget>> callback)
        {
            Try(action,
                (ITypedMemberMappingExceptionContext<TRuntimeSource, TRuntimeTarget> ctx)
                    => callback((ITypedMemberMappingExceptionContext<object, TRuntimeTarget>)ctx));
        }

        public void Try(
            Action action,
            Action<ITypedMemberMappingExceptionContext<TRuntimeSource, TRuntimeTarget>> callback)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                ReportException(callback, ex);
            }
        }

        public TResult Try<TResult>(Func<TResult> funcToTry)
            => Try(funcToTry, (ITypedMemberMappingExceptionContext<TRuntimeSource, TRuntimeTarget> ctx) => { });

        public TResult Try<TResult>(Func<TResult> funcToTry, Action<IUntypedMemberMappingExceptionContext> callback)
        {
            return Try(funcToTry,
                (ITypedMemberMappingExceptionContext<TRuntimeSource, TRuntimeTarget> ctx)
                    => callback((IUntypedMemberMappingExceptionContext)ctx));
        }

        public TResult Try<TResult>(Func<TResult> funcToTry, Action<ITypedMemberMappingExceptionContext<object, TRuntimeTarget>> callback)
        {
            return Try(funcToTry,
                (ITypedMemberMappingExceptionContext<TRuntimeSource, TRuntimeTarget> ctx)
                    => callback((ITypedMemberMappingExceptionContext<object, TRuntimeTarget>)ctx));
        }

        public TResult Try<TResult>(
            Func<TResult> funcToTry,
            Action<ITypedMemberMappingExceptionContext<TRuntimeSource, TRuntimeTarget>> callback)
        {
            try
            {
                return funcToTry.Invoke();
            }
            catch (Exception ex)
            {
                ReportException(callback, ex);
                return default(TResult);
            }
        }

        private void ReportException(
            Action<ITypedMemberMappingExceptionContext<TRuntimeSource, TRuntimeTarget>> callback,
            Exception ex)
        {
            if (callback == null)
            {
                return;
            }

            var exceptionContext = MemberMappingExceptionContext.Create(this, ex);

            callback.Invoke(exceptionContext);
        }

        #endregion

        public TDeclaredMember Map<TDeclaredSource, TDeclaredMember>(
            TDeclaredSource source,
            TDeclaredMember targetMemberValue,
            string targetMemberName,
            int dataSourceIndex)
        {
            var allTargetMembers = GlobalContext.MemberFinder.GetWriteableMembers(_targetMember.Type);
            var targetMember = allTargetMembers.First(tm => tm.Name == targetMemberName);
            var qualifiedTargetMember = _targetMember.Append(targetMember);
            var context = new MemberMappingContext(qualifiedTargetMember, this);
            var sourceMember = context.EnumerateDataSources().ElementAt(dataSourceIndex).SourceMember;

            var targetObjectMappingCommand = ObjectMappingCommand.CreateForChild(
                source,
                sourceMember,
                Target,
                _targetMember,
                targetMemberValue,
                context.TargetMember,
                null,
                MappingContext);

            return targetObjectMappingCommand.Execute();
        }

        public TTargetElement Map<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement existingElement,
            int enumerableIndex)
        {
            var sourceElementMember = _sourceMember.Append(_sourceMember.Type.CreateElementMember());
            var targetElementMember = _targetMember.Append(_targetMember.Type.CreateElementMember());

            var targetElementMappingCommand = ObjectMappingCommand.CreateForChild(
                sourceElement,
                sourceElementMember,
                existingElement,
                targetElementMember,
                existingElement,
                targetElementMember,
                enumerableIndex,
                MappingContext);

            return targetElementMappingCommand.Execute();
        }

        #region IMappingData Members

        string IMappingData.RuleSetName => MappingContext.RuleSet.Name;

        Type IMappingData.SourceType => typeof(TRuntimeSource);

        Type IMappingData.TargetType => typeof(TRuntimeTarget);

        IQualifiedMember IMappingData.TargetMember => _targetMember;

        #endregion

        #region IMemberMappingContext Members

        ParameterExpression IMemberMappingContext.Parameter => _parameter;

        IQualifiedMember IMemberMappingContext.SourceMember => _sourceMember;

        Expression IMemberMappingContext.SourceObject => _sourceObjectProperty;

        int IMemberMappingContext.SourceObjectDepth => _sourceObjectDepth;

        Expression IMemberMappingContext.ExistingObject => _existingObjectProperty;

        Expression IMemberMappingContext.EnumerableIndex => _enumerableIndexProperty;

        ParameterExpression IMemberMappingContext.InstanceVariable => _instanceVariable;

        NestedAccessFinder IMemberMappingContext.NestedAccessFinder => _nestedAccessFinder;

        IEnumerable<IDataSource> IMemberMappingContext.GetDataSources() => this.GetDataSources();

        Expression IMemberMappingContext.WrapInTry(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                return expression;
            }

            var callback = MapperContext.UserConfigurations.GetExceptionCallbackOrNull(this);
            var hasCallback = (callback != null);

            MethodInfo tryMethod;
            Expression tryArgument;

            if (expression.Type == typeof(void))
            {
                tryMethod = GetTryMethod(
                    callback,
                    _tryActionMethod,
                    _tryUntypedHandlerActionMethod,
                    _tryHalfTypedHandlerActionMethod,
                    _tryTypedHandlerActionMethod);

                tryArgument = Expression.Lambda<Action>(expression);
            }
            else
            {
                tryMethod = GetTryMethod(
                    callback,
                    _tryFuncMethod,
                    _tryUntypedHandlerFuncMethod,
                    _tryHalfTypedHandlerFuncMethod,
                    _tryTypedHandlerFuncMethod);

                tryMethod = tryMethod.MakeGenericMethod(expression.Type);
                tryArgument = Expression.Lambda(Expression.GetFuncType(expression.Type), expression);
            }

            var tryCall = hasCallback
                ? Expression.Call(_parameter, tryMethod, tryArgument, callback.Callback)
                : Expression.Call(_parameter, tryMethod, tryArgument);

            return tryCall;
        }

        private static MethodInfo GetTryMethod(
            ExceptionCallback callback,
            MethodInfo noCallbackMethod,
            MethodInfo untypedMethod,
            MethodInfo halfTypedMethod,
            MethodInfo typedMethod)
        {
            return (callback == null)
                ? noCallbackMethod
                : callback.IsUntyped
                    ? untypedMethod
                    : callback.IsSourceTyped
                        ? typedMethod
                        : halfTypedMethod;
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
            if (sourceMember.IsSameAs(_sourceMember))
            {
                return typeof(TRuntimeSource);
            }

            var accessKey = _parameter.Type.FullName + sourceMember.Signature;

            var getRuntimeTypeFunc = GlobalContext.Cache.GetOrAdd(accessKey, k =>
            {
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

                return getRuntimeTypeLambda.Compile();
            });

            return getRuntimeTypeFunc.Invoke(this);
        }

        MethodCallExpression IObjectMappingContext.TryGetCall => _tryGetCall;

        MethodCallExpression IObjectMappingContext.CreateCall => _createCall;

        MethodCallExpression IObjectMappingContext.ObjectRegistrationCall => _registrationCall;

        MethodCallExpression IObjectMappingContext.GetMapCall(
            Expression sourceObject,
            IQualifiedMember objectMember,
            int dataSourceIndex)
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