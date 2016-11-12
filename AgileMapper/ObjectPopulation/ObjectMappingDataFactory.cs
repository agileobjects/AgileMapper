namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;
    using Members.Sources;
    using NetStandardPolyfills;

    internal static class ObjectMappingDataFactory
    {
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
                var typedForChildMethod = typeof(ObjectMappingDataFactory)
                    .GetNonPublicStaticMethod("ForChild")
                    .MakeGenericMethod(k.SourceType, k.TargetType);

                var typedForChildCall = Expression.Call(
                    typedForChildMethod,
                    Expression.Default(k.SourceType),
                    Expression.Default(k.TargetType),
                    Expression.Default(typeof(int?)),
                    Parameters.ChildMembersSource,
                    Parameters.ObjectMappingData);

                var typedForChildLambda = Expression.Lambda<Func<IChildMembersSource, IObjectMappingData, IObjectMappingData>>(
                    typedForChildCall,
                    Parameters.ChildMembersSource,
                    Parameters.ObjectMappingData);

                return typedForChildLambda.Compile();
            });

            var membersSource = new FixedMembersMembersSource(sourceMember, targetMember, dataSourceIndex);

            return typedForChildCaller.Invoke(membersSource, parent);
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

            return ForElement(sourceElementMember, targetElementMember, parent);
        }

        public static IObjectMappingData ForElement(
            IQualifiedMember sourceElementMember,
            QualifiedMember targetElementMember,
            IObjectMappingData parent)
        {
            var sourceType = sourceElementMember.Type;
            var targetType = targetElementMember.Type;
            var key = new SourceAndTargetTypesKey(sourceType, targetType);

            var typedForElementCaller = GlobalContext.Instance.Cache.GetOrAdd(key, k =>
            {
                var typedForElementMethod = typeof(ObjectMappingDataFactory)
                    .GetNonPublicStaticMethod("ForElement")
                    .MakeGenericMethod(k.SourceType, k.TargetType);

                var typedForElementCall = Expression.Call(
                    typedForElementMethod,
                    Expression.Default(k.SourceType),
                    Expression.Default(k.TargetType),
                    Expression.Constant(0, typeof(int?)),
                    Parameters.MembersSource,
                    Parameters.ObjectMappingData);

                var typedForElementLambda = Expression.Lambda<Func<IMembersSource, IObjectMappingData, IObjectMappingData>>(
                    typedForElementCall,
                    Parameters.MembersSource,
                    Parameters.ObjectMappingData);

                return typedForElementLambda.Compile();
            });

            var membersSource = new FixedMembersMembersSource(sourceElementMember, targetElementMember);

            return typedForElementCaller.Invoke(membersSource, parent);
        }

        public static IObjectMappingData ForElement<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
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
            int? enumerableIndex,
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

            var constructionFunc = GetMappingDataCreator<TDeclaredSource, TDeclaredTarget>(mappingTypes);

            return constructionFunc.Invoke(
                source,
                target,
                enumerableIndex,
                mapperKey,
                mappingContext,
                parent);
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
                var dataType = typeof(ObjectMappingData<,>)
                    .MakeGenericType(k.RuntimeSourceType, k.RuntimeTargetType);

                var sourceParameter = Parameters.Create(k.DeclaredSourceType, "source");
                var targetParameter = Parameters.Create(k.DeclaredTargetType, "target");

                var constructorCall = Expression.New(
                    dataType.GetPublicInstanceConstructors().First(),
                    sourceParameter.GetConversionTo(k.RuntimeSourceType),
                    targetParameter.GetConversionTo(k.RuntimeTargetType),
                    Parameters.EnumerableIndexNullable,
                    Parameters.MapperKey,
                    Parameters.MappingContext,
                    Parameters.ObjectMappingData);

                var constructionLambda = Expression.Lambda<MappingDataCreator<TSource, TTarget>>(
                    constructorCall,
                    sourceParameter,
                    targetParameter,
                    Parameters.EnumerableIndexNullable,
                    Parameters.MapperKey,
                    Parameters.MappingContext,
                    Parameters.ObjectMappingData);

                return constructionLambda.Compile();
            });

            return constructionFunc;
        }
    }
}