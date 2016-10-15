namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

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
                (mt, ms, mc) => new RootObjectMapperKey(mc.RuleSet, mt),
                mappingContext.MapperContext.RootMembersSource,
                mappingContext);
        }

        public static IObjectMappingData ForChild<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            string targetMemberName,
            int dataSourceIndex,
            IObjectMappingData parent)
        {
            var memberSource = new ChildMembersSource(parent, targetMemberName, dataSourceIndex);

            return Create(
                source,
                target,
                enumerableIndex,
                (mt, ms, mc) => new ChildObjectMapperKey(ms.TargetMemberName, ms.DataSourceIndex, mt),
                memberSource,
                parent.MappingContext,
                parent);
        }

        public static IObjectMappingData ForElement<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            IObjectMappingData parent)
        {
            return Create(
                source,
                target,
                enumerableIndex,
                (mt, ms, mc) => new ElementObjectMapperKey(mt),
                parent.ElementMembersSource,
                parent.MappingContext,
                parent);
        }

        private static IObjectMappingData Create<TDeclaredSource, TDeclaredTarget, TMemberSource>(
            TDeclaredSource source,
            TDeclaredTarget target,
            int? enumerableIndex,
            Func<MappingTypes, TMemberSource, IMappingContext, ObjectMapperKeyBase> mapperKeyFactory,
            TMemberSource membersSource,
            IMappingContext mappingContext,
            IObjectMappingData parent = null)
            where TMemberSource : IMembersSource
        {
            var mappingTypes = MappingTypes.For(source, target, enumerableIndex, mappingContext, parent);
            var mapperKey = mapperKeyFactory.Invoke(mappingTypes, membersSource, mappingContext);

            if (mappingTypes.RuntimeTypesAreTheSame)
            {
                return new ObjectMappingData<TDeclaredSource, TDeclaredTarget>(
                    source,
                    target,
                    enumerableIndex,
                    mapperKey,
                    membersSource,
                    mappingContext,
                    parent);
            }

            var constructionFunc = GetMappingDataCreator<TDeclaredSource, TDeclaredTarget>(mappingTypes);

            return constructionFunc.Invoke(
                source,
                target,
                enumerableIndex,
                mapperKey,
                membersSource,
                mappingContext,
                parent);
        }

        private delegate IObjectMappingData MappingDataCreator<in TSource, in TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            ObjectMapperKeyBase mapperKey,
            IMembersSource membersSource,
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
                    Parameters.MembersSource,
                    Parameters.MappingContext,
                    Parameters.ObjectMappingData);

                var constructionLambda = Expression.Lambda<MappingDataCreator<TSource, TTarget>>(
                    constructorCall,
                    sourceParameter,
                    targetParameter,
                    Parameters.EnumerableIndexNullable,
                    Parameters.MapperKey,
                    Parameters.MembersSource,
                    Parameters.MappingContext,
                    Parameters.ObjectMappingData);

                return constructionLambda.Compile();
            });

            return constructionFunc;
        }
    }
}