namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;

    internal class ObjectMappingContext<TSource, TTarget> :
        TypedMemberMappingContext<TSource, TTarget>,
        IObjectMappingContext
    {
        #region Cached Items

        private static readonly ParameterExpression _parameter =
            Parameters.Create<ObjectMappingContext<TSource, TTarget>>("omc");

        // ReSharper disable StaticMemberInGenericType
        private static readonly Expression _sourceObjectProperty = Expression.Property(_parameter, "Source");

        private static readonly Expression _targetObjectProperty = Expression.Property(_parameter, "Target");

        private static readonly Expression _createdObjectProperty = Expression.Property(_parameter, "CreatedObject");

        private static readonly Expression _enumerableIndexProperty = Expression.Property(_parameter, "EnumerableIndex");

        private static readonly ParameterExpression _instanceVariable = Expression.Variable(
            typeof(TTarget).IsEnumerable() ? EnumerableTypes.GetEnumerableVariableType(typeof(TTarget)) : typeof(TTarget),
            typeof(TTarget).GetVariableName(f => f.InCamelCase));

        private static readonly NestedAccessFinder _nestedAccessFinder = new NestedAccessFinder(_parameter);

        private static readonly Expression _mappingContextProperty = Expression.Property(_parameter, "MappingContext");

        private static readonly MethodInfo _tryGetMethod = _mappingContextProperty.Type
            .GetMethod("TryGet", Constants.PublicInstance);

        private static readonly MethodCallExpression _tryGetCall = Expression.Call(
            _mappingContextProperty,
            _tryGetMethod.MakeGenericMethod(typeof(TSource), _instanceVariable.Type),
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
            IQualifiedMember sourceMember,
            TSource source,
            QualifiedMember targetMember,
            TTarget target,
            int? enumerableIndex,
            MappingContext mappingContext)
            : base(source, target, enumerableIndex)
        {
            _sourceMember = sourceMember;
            _targetMember = targetMember;
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
            var targetObjectMappingCommand = CreateChildMappingCommand(
                source,
                targetMemberValue,
                targetMemberName,
                dataSourceIndex);

            return targetObjectMappingCommand.Execute();
        }

        public IObjectMappingCommand<TDeclaredMember> CreateChildMappingCommand<TDeclaredSource, TDeclaredMember>(
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

            return ObjectMappingCommand.Create(
                sourceMember,
                source,
                qualifiedTargetMember,
                targetMemberValue,
                GetEnumerableIndex(),
                MappingContext);
        }

        public TTargetElement Map<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement existingElement,
            int enumerableIndex)
            => CreateElementMappingCommand(sourceElement, existingElement, enumerableIndex).Execute();

        public IObjectMappingCommand<TTargetElement> CreateElementMappingCommand<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement existingElement,
            int enumerableIndex)
        {
            var sourceElementMember = _sourceMember.Append(_sourceMember.Type.CreateElementMember());
            var targetElementMember = _targetMember.Append(_targetMember.CreateElementMember());

            return ObjectMappingCommand.Create(
                sourceElementMember,
                sourceElement,
                targetElementMember,
                existingElement,
                enumerableIndex,
                MappingContext);
        }

        public ITypedMemberMappingContext<TContextSource, TContextTarget> AsMemberContext<TContextSource, TContextTarget>()
            => (ITypedMemberMappingContext<TContextSource, TContextTarget>)this;

        #region IMappingData Members

        string IMappingData.RuleSetName => MappingContext.RuleSet.Name;

        Type IMappingData.SourceType => typeof(TSource);

        Type IMappingData.TargetType => typeof(TTarget);

        QualifiedMember IMappingData.TargetMember => _targetMember;

        #endregion

        #region IMemberMappingContext Members

        ParameterExpression IMemberMappingContext.Parameter => _parameter;

        IQualifiedMember IMemberMappingContext.SourceMember => _sourceMember;

        Expression IMemberMappingContext.SourceObject => _sourceObjectProperty;

        Expression IMemberMappingContext.TargetObject => _targetObjectProperty;

        Expression IMemberMappingContext.EnumerableIndex => _enumerableIndexProperty;

        ParameterExpression IMemberMappingContext.InstanceVariable => _instanceVariable;

        NestedAccessFinder IMemberMappingContext.NestedAccessFinder => _nestedAccessFinder;

        #endregion

        #region IObjectMappingContext Members

        T IObjectMappingContext.GetSource<T>() => (T)(object)Source;

        T IObjectMappingContext.GetTarget<T>() => (T)(object)Target;

        Expression IObjectMappingContext.CreatedObject => _createdObjectProperty;

        public int? GetEnumerableIndex() => EnumerableIndex ?? Parent?.GetEnumerableIndex();

        Type IObjectMappingContext.GetSourceMemberRuntimeType(IQualifiedMember sourceMember)
        {
            if (Source == null)
            {
                return sourceMember.Type;
            }

            if (sourceMember == _sourceMember)
            {
                return typeof(TSource);
            }

            var accessKey = sourceMember.Signature + ": GetRuntimeSourceType";

            var getRuntimeTypeFunc = GlobalContext.Cache.GetOrAdd(accessKey, k =>
            {
                var sourceParameter = Parameters.Create<TSource>("source");
                var relativeMember = sourceMember.RelativeTo(_sourceMember);
                var memberAccess = relativeMember.GetQualifiedAccess(_sourceObjectProperty);
                memberAccess = memberAccess.Replace(_sourceObjectProperty, sourceParameter);

                var getRuntimeTypeCall = Expression.Call(
                    ObjectExtensions.GetRuntimeSourceTypeMethod.MakeGenericMethod(sourceMember.Type),
                    memberAccess);

                var getRuntimeTypeLambda = Expression
                    .Lambda<Func<TSource, Type>>(getRuntimeTypeCall, sourceParameter);

                return getRuntimeTypeLambda.Compile();
            });

            return getRuntimeTypeFunc.Invoke(Source);
        }

        MethodCallExpression IObjectMappingContext.TryGetCall => _tryGetCall;

        MethodCallExpression IObjectMappingContext.ObjectRegistrationCall => _registrationCall;

        MethodCallExpression IObjectMappingContext.GetMapCall(
            Expression sourceObject,
            QualifiedMember targetMember,
            int dataSourceIndex)
        {
            var mapCall = Expression.Call(
                _parameter,
                _mapObjectMethod.MakeGenericMethod(sourceObject.Type, targetMember.Type),
                sourceObject,
                targetMember.GetAccess(_instanceVariable),
                Expression.Constant(targetMember.Name),
                Expression.Constant(dataSourceIndex));

            return mapCall;
        }

        MethodCallExpression IObjectMappingContext.GetMapCall(Expression sourceElement, Expression existingElement)
        {
            var mapCall = Expression.Call(
                _parameter,
                _mapEnumerableElementMethod.MakeGenericMethod(sourceElement.Type, existingElement.Type),
                sourceElement,
                existingElement,
                Parameters.EnumerableIndex);

            return mapCall;
        }

        #endregion

        #region IObjectCreationContext

        public TTarget CreatedObject { get; set; }

        #endregion
    }
}