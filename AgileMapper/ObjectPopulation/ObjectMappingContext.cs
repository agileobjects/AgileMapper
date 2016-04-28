namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;

    internal class ObjectMappingContext<TRuntimeSource, TRuntimeTarget> : IObjectMappingContext
    {
        #region Cached Items

        private static readonly ParameterExpression _parameter = Expression.Parameter(
            typeof(ObjectMappingContext<TRuntimeSource, TRuntimeTarget>),
            "oc");

        private static readonly MethodInfo _createMethod =
            typeof(ObjectMappingContext<TRuntimeSource, TRuntimeTarget>)
                .GetMethod("Create", Constants.PublicInstance);

        private static readonly MethodInfo _mapComplexTypeMethod =
            typeof(ObjectMappingContext<TRuntimeSource, TRuntimeTarget>)
                .GetMethods(Constants.PublicInstance)
                .First(m => m.Name == "Map" && m.GetParameters().Length == 1);

        private static readonly MethodInfo _mapEnumerableMethod =
            typeof(ObjectMappingContext<TRuntimeSource, TRuntimeTarget>)
                .GetMethods(Constants.PublicInstance)
                .First(m => m.Name == "Map" && m.GetParameters().Length == 2);

        private static readonly MethodInfo _mapEnumerableElementMethod =
            typeof(ObjectMappingContext<TRuntimeSource, TRuntimeTarget>)
                .GetMethods(Constants.PublicInstance)
                .First(m => m.Name == "Map" && m.GetParameters().Length == 3);

        #endregion

        private readonly IObjectMappingContext _parent;
        private readonly Expression _sourceObject;
        private readonly int _sourceObjectDepth;
        private readonly Expression _existingObject;
        private readonly ParameterExpression _targetVariable;
        private readonly QualifiedMember _qualifiedTargetMember;

        public ObjectMappingContext(
            Member targetMember,
            TRuntimeSource source,
            TRuntimeTarget existing,
            int? enumerableIndex,
            MappingContext mappingContext)
        {
            MappingContext = mappingContext;
            Source = source;
            Existing = existing;

            _parent = mappingContext.CurrentObjectMappingContext;
            _sourceObject = Expression.Property(_parameter, "Source");
            _sourceObjectDepth = CalculateSourceObjectDepth();
            _existingObject = Expression.Property(_parameter, "Existing");

            var targetVariableType = typeof(TRuntimeTarget).GetTargetVariableType();
            _targetVariable = Expression.Variable(targetVariableType, "target");

            _qualifiedTargetMember =
                _parent?.TargetMember.Append(targetMember)
                    ?? QualifiedMember.From(targetMember);
        }

        private int CalculateSourceObjectDepth()
        {
            var parent = _parent;

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

        public MappingContext MappingContext { get; }

        public TRuntimeSource Source { get; }

        public TRuntimeTarget Existing { get; }

        public TRuntimeTarget Create()
        {
            return MappingContext.MapperContext.ComplexTypeFactory.Create<TRuntimeTarget>();
        }

        public TMember Map<TMember>(Expression<Func<TRuntimeTarget, TMember>> complexChildMember)
        {
            return MappingContext.MapChild(Source, Existing, complexChildMember);
        }

        public TMember Map<TDeclaredSource, TMember>(
            TDeclaredSource sourceEnumerable,
            Expression<Func<TRuntimeTarget, TMember>> enumerableChildMember)
        {
            return MappingContext.MapChild(sourceEnumerable, Existing, enumerableChildMember);
        }

        public TTargetElement Map<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement existingElement,
            int enumerableIndex)
        {
            return MappingContext.MapEnumerableElement(sourceElement, existingElement, enumerableIndex);
        }

        #region IObjectMappingContext Members

        IObjectMappingContext IObjectMappingContext.Parent => _parent;

        ParameterExpression IObjectMappingContext.Parameter => _parameter;

        bool IObjectMappingContext.HasSource<TSource>(TSource source)
        {
            return ReferenceEquals(Source, source);
        }

        Expression IObjectMappingContext.SourceObject => _sourceObject;

        int IObjectMappingContext.SourceObjectDepth => _sourceObjectDepth;

        Expression IObjectMappingContext.ExistingObject => _existingObject;

        ParameterExpression IObjectMappingContext.TargetVariable => _targetVariable;

        QualifiedMember IObjectMappingContext.TargetMember => _qualifiedTargetMember;

        MethodCallExpression IObjectMappingContext.GetCreateCall() => Expression.Call(_parameter, _createMethod);

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
            var targetObjectParameter = Expression.Parameter(typeof(TRuntimeTarget), "x");
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