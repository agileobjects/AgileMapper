namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;
    using Members;
    using Members.Sources;
    using NetStandardPolyfills;

    internal class ObjectMappingDataFactory : IObjectMappingDataFactoryBridge
    {
        private static readonly IObjectMappingDataFactoryBridge _bridge = new ObjectMappingDataFactory();

        public static IObjectMappingData ForRoot<TSource, TTarget>(
            TSource source,
            TTarget target,
            IMappingContext mappingContext)
        {
            return Create(
                source,
                target,
                null,
                (mt, mc) => new RootObjectMapperKey(mt, mc),
                mappingContext);
        }

        public static IObjectMappingData ForChild(
            string targetMemberRegistrationName,
            int dataSourceIndex,
            IObjectMappingData parent)
        {
            var sourceMember = parent.MapperData.GetSourceMemberFor(targetMemberRegistrationName, dataSourceIndex);
            var targetMember = parent.MapperData.GetTargetMemberFor(targetMemberRegistrationName);

            return ForChild(sourceMember, targetMember, dataSourceIndex, parent);
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
                    .GetMethod("ForChild")
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
            return ForChild(
                default(TSource),
                default(TTarget),
                default(int?),
                (IChildMembersSource)childMembersSource,
                (IObjectMappingData)parent);
        }

        public static IObjectMappingData ForChild<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            string targetMemberRegistrationName,
            int dataSourceIndex,
            IObjectMappingData parent)
        {
            var membersSource = new MemberLookupsChildMembersSource(parent, targetMemberRegistrationName, dataSourceIndex);

            return ForChild(
                source,
                target,
                enumerableIndex,
                membersSource,
                parent);
        }

        private static IObjectMappingData ForChild<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            IChildMembersSource membersSource,
            IObjectMappingData parent)
        {
            return Create(
                source,
                target,
                enumerableIndex,
                (mt, mc) => new ChildObjectMapperKey(mt, membersSource),
                parent.MappingContext,
                parent);
        }

        public static IObjectMappingData ForElement(IObjectMappingData parent)
        {
            var sourceElementMember = parent.MapperData.SourceMember.GetElementMember();
            var targetElementMember = parent.MapperData.TargetMember.GetElementMember();

            var key = new SourceAndTargetTypesKey(sourceElementMember.Type, targetElementMember.Type);

            var typedForElementCaller = GlobalContext.Instance.Cache.GetOrAdd(key, k =>
            {
                var bridgeParameter = Expression.Parameter(typeof(IObjectMappingDataFactoryBridge), "bridge");
                var membersSourceParameter = Expression.Parameter(typeof(object), "membersSource");
                var parentParameter = Expression.Parameter(typeof(object), "parent");

                var typedForElementMethod = bridgeParameter.Type
                    .GetMethod("ForElement")
                    .MakeGenericMethod(k.SourceType, k.TargetType);

                var typedForElementCall = Expression.Call(
                    bridgeParameter,
                    typedForElementMethod,
                    membersSourceParameter,
                    parentParameter);

                var typedForElementLambda = Expression.Lambda<Func<IObjectMappingDataFactoryBridge, object, object, object>>(
                    typedForElementCall,
                    bridgeParameter,
                    membersSourceParameter,
                    parentParameter);

                return typedForElementLambda.Compile();
            });

            var membersSource = new FixedMembersMembersSource(sourceElementMember, targetElementMember);

            return (IObjectMappingData)typedForElementCaller.Invoke(_bridge, membersSource, parent);
        }

        object IObjectMappingDataFactoryBridge.ForElement<TSource, TTarget>(object membersSource, object parent)
        {
            return ForElement(
                default(TSource),
                default(TTarget),
                0,
                (IMembersSource)membersSource,
                (IObjectMappingData)parent);
        }

        public static IObjectMappingData ForElement<TSource, TTarget>(
            TSource source,
            TTarget target,
            int enumerableIndex,
            IObjectMappingData parent)
        {
            var membersSource = new ElementMembersSource(parent);

            return ForElement(
                source,
                target,
                enumerableIndex,
                membersSource,
                parent);
        }

        private static IObjectMappingData ForElement<TSource, TTarget>(
            TSource source,
            TTarget target,
            int enumerableIndex,
            IMembersSource membersSource,
            IObjectMappingData parent)
        {
            return Create(
                source,
                target,
                enumerableIndex,
                (mt, mc) => new ElementObjectMapperKey(mt, membersSource),
                parent.MappingContext,
                parent);
        }

        private static IObjectMappingData Create<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget target,
            int? enumerableIndex,
            Func<MappingTypes, IMappingContext, ObjectMapperKeyBase> mapperKeyFactory,
            IMappingContext mappingContext,
            IObjectMappingData parent = null)
        {
            var mappingTypes = MappingTypes.For(source, target);
            var mapperKey = mapperKeyFactory.Invoke(mappingTypes, mappingContext);

            if (mappingTypes.RuntimeTypesAreTheSame)
            {
                return new ObjectMappingData<TDeclaredSource, TDeclaredTarget>(
                    source,
                    target,
                    enumerableIndex,
                    mapperKey,
                    mappingContext,
                    parent);
            }

            if (Constants.IsPartialTrust)
            {
                var createCaller = GetPartialTrustMappingDataCreator<TDeclaredSource, TDeclaredTarget>(mappingTypes);

                return (IObjectMappingData)createCaller.Invoke(
                    _bridge,
                    source,
                    target,
                    enumerableIndex,
                    mapperKey,
                    mappingContext,
                    parent);
            }

            var constructionFunc = GetMappingDataCreator<TDeclaredSource, TDeclaredTarget>(mappingTypes);

            return constructionFunc.Invoke(
                source,
                target,
                enumerableIndex,
                mapperKey,
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
                var mapperKeyParameter = Expression.Parameter(typeof(object), "mapperKey");
                var mappingContextParameter = Expression.Parameter(typeof(object), "mappingContext");
                var parentParameter = Expression.Parameter(typeof(object), "parent");

                var createMethod = bridgeParameter.Type
                    .GetMethod("CreateMappingData")
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
                    mapperKeyParameter,
                    mappingContextParameter,
                    parentParameter);

                var createLambda = Expression
                    .Lambda<Func<IObjectMappingDataFactoryBridge, TSource, TTarget, int?, object, object, object, object>>(
                        createCall,
                        bridgeParameter,
                        sourceParameter,
                        targetParameter,
                        enumerableIndexParameter,
                        mapperKeyParameter,
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
            object mapperKey,
            object mappingContext,
            object parent)
        {
            return new ObjectMappingData<TSource, TTarget>(
                (TSource)source,
                (TTarget)target,
                enumerableIndex,
                (ObjectMapperKeyBase)mapperKey,
                (IMappingContext)mappingContext,
                (IObjectMappingData)parent);
        }

        private delegate IObjectMappingData MappingDataCreator<in TSource, in TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            ObjectMapperKeyBase mapperKey,
            IMappingContext mappingContext,
            IObjectMappingData parent);

        private static MappingDataCreator<TSource, TTarget> GetMappingDataCreator<TSource, TTarget>(
            MappingTypes mappingTypes)
        {
            var constructorKey = DeclaredAndRuntimeTypesKey.For<TSource, TTarget>(mappingTypes);

            // TODO: Local cache
            var constructionFunc = GlobalContext.Instance.Cache.GetOrAdd(constructorKey, k =>
            {
                var mapperKeyParameter = Expression.Parameter(typeof(ObjectMapperKeyBase), "mapperKey");
                var enumerableIndexParameter = Expression.Parameter(typeof(int?), "i");

                var dataType = typeof(ObjectMappingData<,>)
                    .MakeGenericType(k.RuntimeSourceType, k.RuntimeTargetType);

                var sourceParameter = Parameters.Create(k.DeclaredSourceType, "source");
                var targetParameter = Parameters.Create(k.DeclaredTargetType, "target");

                var constructorCall = Expression.New(
                    dataType.GetPublicInstanceConstructors().First(),
                    sourceParameter.GetConversionTo(k.RuntimeSourceType),
                    targetParameter.GetConversionTo(k.RuntimeTargetType),
                    enumerableIndexParameter,
                    mapperKeyParameter,
                    Parameters.MappingContext,
                    Parameters.ObjectMappingData);

                var constructionLambda = Expression.Lambda<MappingDataCreator<TSource, TTarget>>(
                    constructorCall,
                    sourceParameter,
                    targetParameter,
                    enumerableIndexParameter,
                    mapperKeyParameter,
                    Parameters.MappingContext,
                    Parameters.ObjectMappingData);

                return constructionLambda.Compile();
            });

            return constructionFunc;
        }
    }
}