namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Dynamic;
    using System.Linq;
    using Enumerables;
    using Extensions.Internal;
    using MapperKeys;
    using Members;
    using Members.Sources;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
    using Microsoft.Scripting.Utils;
#else
    using System.Linq.Expressions;
#endif

    internal class ObjectMappingDataFactory : IObjectMappingDataFactoryBridge
    {
        private static readonly IObjectMappingDataFactoryBridge _bridge = new ObjectMappingDataFactory();

        public static ObjectMappingData<TSource, TTarget> ForRootFixedTypes<TSource, TTarget>(
            IMappingContext mappingContext)
        {
            return ForRootFixedTypes(default(TSource), default(TTarget), mappingContext);
        }

        public static ObjectMappingData<IQueryable<TSourceElement>, IQueryable<TResultElement>> ForProjection<TSourceElement, TResultElement>(
            IQueryable<TSourceElement> sourceQueryable,
            IMappingContext mappingContext)
        {
            return ForRootFixedTypes(
                sourceQueryable,
                default(IQueryable<TResultElement>),
                MappingTypes<TSourceElement, TResultElement>.Fixed,
                mappingContext);
        }

        public static ObjectMappingData<TSource, TTarget> ForRootFixedTypes<TSource, TTarget>(
            TSource source,
            TTarget target,
            IMappingContext mappingContext)
        {
            return ForRootFixedTypes(
                source,
                target,
                MappingTypes<TSource, TTarget>.Fixed,
                mappingContext);
        }

        private static ObjectMappingData<TSource, TTarget> ForRootFixedTypes<TSource, TTarget>(
            TSource source,
            TTarget target,
            MappingTypes mappingTypes,
            IMappingContext mappingContext)
        {
            return new ObjectMappingData<TSource, TTarget>(
                source,
                target,
                null, // <- No enumerable index because we're at the root
                mappingTypes,
                mappingContext,
                parent: null);
        }

        public static IObjectMappingData ForRoot<TSource, TTarget>(
            TSource source,
            TTarget target,
            IMappingContext mappingContext)
        {
            MappingTypes mappingTypes;

            if ((target == null) && (typeof(TTarget) == typeof(object)))
            {
                // This is a 'create new' mapping where the target type has come 
                // through as 'object'. This happens when you use .ToANew<dynamic>(),
                // and I can't see how to differentiate that from .ToANew<object>().
                // Given that the former is more likely and that people asking for 
                // .ToANew<object>() are doing something weird, default the target 
                // type to ExpandoObject:
                mappingTypes = MappingTypes.For(source, default(ExpandoObject));

                return Create(
                    source,
                    default(ExpandoObject),
                    null,
                    mappingTypes,
                    mappingContext);
            }

            mappingTypes = MappingTypes.For(source, target);

            return Create(
                source,
                target,
                null,
                mappingTypes,
                mappingContext);
        }

        public static IObjectMappingData ForChild(
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            int dataSourceIndex,
            IObjectMappingData parent)
        {
            var key = new SourceAndTargetTypesKey(sourceMember.Type, targetMember.Type);

            var typedForChildCaller = GlobalContext.Instance.Cache.GetOrAdd(key, k =>
            {
                var bridgeParameter = Expression.Parameter(typeof(IObjectMappingDataFactoryBridge), "bridge");
                var childMembersSourceParameter = Expression.Parameter(typeof(object), "childMembersSource");
                var parentParameter = Expression.Parameter(typeof(object), "parent");

                var typedForChildMethod = bridgeParameter.Type
                    .GetPublicInstanceMethod("ForChild")
                    .MakeGenericMethod(k.SourceType, k.TargetType);

                var typedForChildCall = Expression.Call(
                    bridgeParameter,
                    typedForChildMethod,
                    childMembersSourceParameter,
                    parentParameter);

                var typedForChildLambda = Expression.Lambda<Func<IObjectMappingDataFactoryBridge, object, object, object>>(
                    typedForChildCall,
                    bridgeParameter,
                    childMembersSourceParameter,
                    parentParameter);

                return typedForChildLambda.Compile();
            });

            var membersSource = new FixedMembersMembersSource(sourceMember, targetMember, dataSourceIndex);

            return (IObjectMappingData)typedForChildCaller.Invoke(_bridge, membersSource, parent);
        }

        object IObjectMappingDataFactoryBridge.ForChild<TSource, TTarget>(object childMembersSource, object parent)
        {
            var mapperKey = new ChildObjectMapperKey(
                MappingTypes.For(default(TSource), default(TTarget)),
                (IChildMembersSource)childMembersSource);

            var parentMappingData = (IObjectMappingData)parent;

            return Create(
                default(TSource),
                default(TTarget),
                default(int?),
                mapperKey,
                parentMappingData);
        }

        public static IObjectMappingData ForChild<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            string targetMemberRegistrationName,
            int dataSourceIndex,
            IObjectMappingData parent)
        {
            var mapperKey = new ChildObjectMapperKey(
                MappingTypes.For(source, target),
                targetMemberRegistrationName,
                dataSourceIndex);

            return Create(
                source,
                target,
                enumerableIndex,
                mapperKey,
                parent);
        }

        public static IObjectMappingData ForElement(IObjectMappingData parent)
        {
            var sourceElementType = parent.MapperData.SourceMember.GetElementMember().Type;
            var targetElementType = parent.MapperData.TargetMember.GetElementMember().Type;

            return ForElement(sourceElementType, targetElementType, parent);
        }

        public static IObjectMappingData ForElement(
            Type sourceElementType,
            Type targetElementType,
            IObjectMappingData parent)
        {
            var key = new ForElementCallerKey(sourceElementType, targetElementType);

            var typedForElementCaller = GlobalContext.Instance.Cache.GetOrAdd(key, k =>
            {
                var bridgeParameter = Expression.Parameter(typeof(IObjectMappingDataFactoryBridge), "bridge");
                var parentParameter = Expression.Parameter(typeof(object), "parent");

                var typedForElementMethod = bridgeParameter.Type
                    .GetPublicInstanceMethod("ForElement")
                    .MakeGenericMethod(k.SourceType, k.TargetType);

                var typedForElementCall = Expression.Call(
                    bridgeParameter,
                    typedForElementMethod,
                    parentParameter);

                var typedForElementLambda = Expression.Lambda<Func<IObjectMappingDataFactoryBridge, object, object>>(
                    typedForElementCall,
                    bridgeParameter,
                    parentParameter);

                return typedForElementLambda.Compile();
            });

            return (IObjectMappingData)typedForElementCaller.Invoke(_bridge, parent);
        }

        object IObjectMappingDataFactoryBridge.ForElement<TSource, TTarget>(object parent)
        {
            var mappingData = (IObjectMappingData)parent;
            var source = mappingData.GetSource<TSource>();
            var target = mappingData.GetTarget<TTarget>();
            var index = mappingData.GetEnumerableIndex().GetValueOrDefault();

            return ForElement(source, target, index, mappingData);
        }

        public static IObjectMappingData ForElement<TSource, TTarget>(
            TSource source,
            TTarget target,
            int enumerableIndex,
            IObjectMappingData parent)
        {
            var mapperKey = new ElementObjectMapperKey(MappingTypes.For(source, target));

            return Create(
                source,
                target,
                enumerableIndex,
                mapperKey,
                parent);
        }

        private static IObjectMappingData Create<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget target,
            int? enumerableIndex,
            ObjectMapperKeyBase mapperKey,
            IObjectMappingData parent)
        {
            var mappingData = Create(
                source,
                target,
                enumerableIndex,
                mapperKey.MappingTypes,
                parent.MappingContext,
                parent);

            mappingData.MapperKey = mapperKey;

            return mappingData;
        }

        private static IObjectMappingData Create<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget target,
            int? enumerableIndex,
            MappingTypes mappingTypes,
            IMappingContext mappingContext,
            IObjectMappingData parent = null)
        {
            if (mappingTypes.RuntimeTypesAreTheSame)
            {
                return new ObjectMappingData<TDeclaredSource, TDeclaredTarget>(
                    source,
                    target,
                    enumerableIndex,
                    mappingTypes,
                    mappingContext,
                    parent);
            }

            if (Constants.ReflectionNotPermitted)
            {
                var createCaller = GetPartialTrustMappingDataCreator<TDeclaredSource, TDeclaredTarget>(mappingTypes);

                return (IObjectMappingData)createCaller.Invoke(
                    _bridge,
                    source,
                    target,
                    enumerableIndex,
                    mappingTypes,
                    mappingContext,
                    parent);
            }

            var constructionFunc = GetMappingDataCreator<TDeclaredSource, TDeclaredTarget>(mappingTypes);

            return constructionFunc.Invoke(
                source,
                target,
                enumerableIndex,
                mappingTypes,
                mappingContext,
                parent);
        }

        private static Func<IObjectMappingDataFactoryBridge, TSource, TTarget, int?, object, object, object, object> GetPartialTrustMappingDataCreator<TSource, TTarget>(
            MappingTypes mappingTypes)
        {
            var createCallerKey = DeclaredAndRuntimeTypesKey.For<TSource, TTarget>(mappingTypes);

            var createCallerFunc = GlobalContext.Instance.Cache.GetOrAdd(createCallerKey, k =>
            {
                var bridgeParameter = Expression.Parameter(typeof(IObjectMappingDataFactoryBridge), "bridge");
                var sourceParameter = Parameters.Create(k.DeclaredSourceType, "source");
                var targetParameter = Parameters.Create(k.DeclaredTargetType, "target");
                var enumerableIndexParameter = Expression.Parameter(typeof(int?), "i");
                var mappingTypesParameter = Expression.Parameter(typeof(object), "mappingTypes");
                var mappingContextParameter = Expression.Parameter(typeof(object), "mappingContext");
                var parentParameter = Expression.Parameter(typeof(object), "parent");

                var createMethod = bridgeParameter.Type
                    .GetPublicInstanceMethod("CreateMappingData")
                    .MakeGenericMethod(
                        k.DeclaredSourceType,
                        k.DeclaredTargetType,
                        k.RuntimeSourceType,
                        k.RuntimeTargetType);

                var createCall = Expression.Call(
                    bridgeParameter,
                    createMethod,
                    sourceParameter,
                    targetParameter,
                    enumerableIndexParameter,
                    mappingTypesParameter,
                    mappingContextParameter,
                    parentParameter);

                var createLambda = Expression
                    .Lambda<Func<IObjectMappingDataFactoryBridge, TSource, TTarget, int?, object, object, object, object>>(
                        createCall,
                        bridgeParameter,
                        sourceParameter,
                        targetParameter,
                        enumerableIndexParameter,
                        mappingTypesParameter,
                        mappingContextParameter,
                        parentParameter);

                return createLambda.Compile();
            });

            return createCallerFunc;
        }

        object IObjectMappingDataFactoryBridge.CreateMappingData<TDeclaredSource, TDeclaredTarget, TSource, TTarget>(
            TDeclaredSource source,
            TDeclaredTarget target,
            int? enumerableIndex,
            object mappingTypes,
            object mappingContext,
            object parent)
        {
            return new ObjectMappingData<TSource, TTarget>(
                (TSource)source,
                (TTarget)target,
                enumerableIndex,
                (MappingTypes)mappingTypes,
                (IMappingContext)mappingContext,
                (IObjectMappingData)parent);
        }

        private delegate IObjectMappingData MappingDataCreator<in TSource, in TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            MappingTypes mappingTypes,
            IMappingContext mappingContext,
            IObjectMappingData parent);

        private static MappingDataCreator<TSource, TTarget> GetMappingDataCreator<TSource, TTarget>(
            MappingTypes mappingTypes)
        {
            var constructorKey = DeclaredAndRuntimeTypesKey.For<TSource, TTarget>(mappingTypes);

            var constructionFunc = GlobalContext.Instance.Cache.GetOrAdd(constructorKey, k =>
            {
                var mappingTypesParameter = typeof(MappingTypes).GetOrCreateParameter("mappingTypes");
                var mappingContextParameter = typeof(IMappingContext).GetOrCreateParameter("mappingContext");
                var mappingDataParameter = typeof(IObjectMappingData).GetOrCreateParameter("mappingData");
                var enumerableIndexParameter = Expression.Parameter(typeof(int?), "i");

                var dataType = typeof(ObjectMappingData<,>)
                    .MakeGenericType(k.RuntimeSourceType, k.RuntimeTargetType);

                var sourceParameter = Parameters.Create(k.DeclaredSourceType, "source");
                var targetParameter = Parameters.Create(k.DeclaredTargetType, "target");

                var targetParameterValue = TypeExtensionsPolyfill.IsPrimitive(k.RuntimeTargetType)
                    ? Expression.Coalesce(targetParameter, typeof(int).ToDefaultExpression())
                    : (Expression)targetParameter;

                var constructorCall = Expression.New(
                    dataType.GetPublicInstanceConstructors().First(),
                    sourceParameter.GetConversionTo(k.RuntimeSourceType),
                    targetParameterValue.GetConversionTo(k.RuntimeTargetType),
                    enumerableIndexParameter,
                    mappingTypesParameter,
                    mappingContextParameter,
                    mappingDataParameter);

                var constructionLambda = Expression.Lambda<MappingDataCreator<TSource, TTarget>>(
                    constructorCall,
                    sourceParameter,
                    targetParameter,
                    enumerableIndexParameter,
                    mappingTypesParameter,
                    mappingContextParameter,
                    mappingDataParameter);

                return constructionLambda.Compile();
            });

            return constructionFunc;
        }

        #region Key Classes

        private class ForElementCallerKey : SourceAndTargetTypesKey
        {
            public ForElementCallerKey(Type sourceType, Type targetType)
                : base(sourceType, targetType)
            {
            }
        }

        #endregion
    }
}