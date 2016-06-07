namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using DataSources;
    using Extensions;
    using Members;

    internal class ObjectMappingContext<TRuntimeSource, TRuntimeTarget, TObject> :
        TypedMemberMappingContext<TRuntimeSource, TRuntimeTarget>,
        ITypedObjectCreationMappingContext<TRuntimeSource, TRuntimeTarget, TObject>,
        IObjectMappingContext
    {
        #region Cached Items

        private static readonly ParameterExpression _parameter =
            Parameters.Create<ObjectMappingContext<TRuntimeSource, TRuntimeTarget, TObject>>("omc");

        // ReSharper disable StaticMemberInGenericType
        private static readonly Expression _sourceObjectProperty = Expression.Property(_parameter, "Source");

        private static readonly Expression _existingObjectProperty = Expression.Property(_parameter, "ExistingObject");

        private static readonly Expression _createdObjectProperty = Expression.Property(_parameter, "CreatedObject");

        private static readonly Expression _enumerableIndexProperty = Expression.Property(_parameter, "EnumerableIndex");

        private static readonly ParameterExpression _instanceVariable = Expression.Variable(
            typeof(TObject).IsEnumerable() ? EnumerableTypes.GetEnumerableVariableType<TObject>() : typeof(TObject),
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
        private readonly QualifiedMember _targetMember;

        public ObjectMappingContext(
            TRuntimeSource source,
            IQualifiedMember sourceMember,
            TRuntimeTarget target,
            QualifiedMember targetMember,
            TObject existingObject,
            int? enumerableIndex,
            MappingContext mappingContext)
            : base(source, target, enumerableIndex)
        {
            _sourceMember = sourceMember;
            _targetMember = targetMember;
            ExistingObject = existingObject;
            MappingContext = mappingContext;
            Parent = mappingContext.CurrentObjectMappingContext;
        }

        public GlobalContext GlobalContext => MapperContext.GlobalContext;

        public MapperContext MapperContext => MappingContext.MapperContext;

        public MappingContext MappingContext { get; }

        IMappingData IMappingData.Parent => Parent;

        IObjectMappingContext IMemberMappingContext.Parent => Parent;

        public IObjectMappingContext Parent { get; }

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
            var sourceMember = context.DataSourceAt(dataSourceIndex).SourceMember;

            var targetObjectMappingCommand = ObjectMappingCommand.CreateForChild(
                source,
                sourceMember,
                Target,
                _targetMember,
                targetMemberValue,
                context.TargetMember,
                GetEnumerableIndex(),
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

        QualifiedMember IMappingData.TargetMember => _targetMember;

        #endregion

        #region IMemberMappingContext Members

        ParameterExpression IMemberMappingContext.Parameter => _parameter;

        IQualifiedMember IMemberMappingContext.SourceMember => _sourceMember;

        Expression IMemberMappingContext.SourceObject => _sourceObjectProperty;

        Expression IMemberMappingContext.ExistingObject => _existingObjectProperty;

        Expression IMemberMappingContext.EnumerableIndex => _enumerableIndexProperty;

        ParameterExpression IMemberMappingContext.InstanceVariable => _instanceVariable;

        NestedAccessFinder IMemberMappingContext.NestedAccessFinder => _nestedAccessFinder;

        DataSourceSet IMemberMappingContext.GetDataSources() => this.GetDataSources();

        #endregion

        #region IObjectMappingContext Members

        bool IObjectMappingContext.HasSource<TSource>(TSource source)
        {
            return ReferenceEquals(Source, source);
        }

        T IObjectMappingContext.GetInstance<T>() => (T)((object)CreatedObject ?? ExistingObject);

        Expression IObjectMappingContext.CreatedObject => _createdObjectProperty;

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
                var relativeMember = sourceMember.RelativeTo(_sourceMember);
                var memberAccess = relativeMember.GetQualifiedAccess(_sourceObjectProperty);

                var getRuntimeTypeCall = Expression.Call(
                    typeof(ObjectExtensions)
                        .GetMethod("GetRuntimeSourceType", Constants.PublicStatic)
                        .MakeGenericMethod(sourceMember.Type),
                    memberAccess);

                var getRuntimeTypeLambda = Expression
                    .Lambda<Func<ObjectMappingContext<TRuntimeSource, TRuntimeTarget, TObject>, Type>>(
                        getRuntimeTypeCall,
                        _parameter);

                return getRuntimeTypeLambda.Compile();
            });

            return getRuntimeTypeFunc.Invoke(this);
        }

        MethodCallExpression IObjectMappingContext.TryGetCall => _tryGetCall;

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

        #region ITypedObjectMappingContext

        public TObject ExistingObject { get; }

        public TObject CreatedObject { get; set; }

        #endregion
    }
}