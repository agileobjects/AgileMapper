namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal static class MapperCreationDataFactory
    {
        private delegate IObjectMapperCreationData MappingDataCreator<in TSource, in TTarget>(
            MappingContext mappingContext,
            TSource source,
            TTarget target,
            int? enumerableIndex,
            ObjectMapperData mapperData);

        public static IObjectMapperCreationData CreateRoot<TDeclaredSource, TDeclaredTarget>(
            MappingContext mappingContext,
            TDeclaredSource source,
            TDeclaredTarget target)
        {
            var rootInstanceData = new MappingInstanceData<TDeclaredSource, TDeclaredTarget>(
                mappingContext,
                source,
                target);

            var sourceMember = mappingContext.MapperContext.RootMemberFactory.RootSource<TDeclaredSource>();
            var targetMember = mappingContext.MapperContext.RootMemberFactory.RootTarget<TDeclaredTarget>();

            var bridge = ObjectMapperDataBridge.Create(
                rootInstanceData,
                sourceMember,
                targetMember);

            return Create(bridge);
        }

        public static IObjectMapperCreationData Create<TDeclaredSource, TDeclaredTarget>(
            ObjectMapperDataBridge<TDeclaredSource, TDeclaredTarget> bridge)
        {
            var mapperData = GetObjectMapperData(bridge);

            if (bridge.RuntimeTypesAreTheSame)
            {
                return new ObjectMappingData<TDeclaredSource, TDeclaredTarget>(bridge.InstanceData, mapperData);
            }

            var constructionFunc = GetMappingDataCreator(bridge);

            return constructionFunc.Invoke(
                bridge.MappingContext,
                bridge.InstanceData.Source,
                bridge.InstanceData.Target,
                bridge.InstanceData.EnumerableIndex,
                mapperData);
        }

        private static ObjectMapperData GetObjectMapperData<TDeclaredSource, TDeclaredTarget>(
            ObjectMapperDataBridge<TDeclaredSource, TDeclaredTarget> bridge)
        {
            var mapperDataKey = ObjectMapperKey.For(bridge);

            var mapperData = bridge.MappingContext.MapperContext.Cache
                .GetOrAdd(mapperDataKey, bridge.GetMapperData);

            return mapperData;
        }

        private static MappingDataCreator<TDeclaredSource, TDeclaredTarget> GetMappingDataCreator<TDeclaredSource, TDeclaredTarget>(
            ObjectMapperDataBridge<TDeclaredSource, TDeclaredTarget> bridge)
        {
            var constructorKey = DeclaredAndRuntimeTypesKey.ForMappingDataConstructor(bridge);

            var constructionFunc = GlobalContext.Instance.Cache.GetOrAdd(constructorKey, _ =>
            {
                var dataType = typeof(ObjectMappingData<,>)
                    .MakeGenericType(bridge.SourceMember.Type, bridge.TargetMember.Type);

                var sourceParameter = Parameters.Create<TDeclaredSource>("source");
                var targetParameter = Parameters.Create<TDeclaredTarget>("target");

                var constructorCall = Expression.New(
                    dataType.GetPublicInstanceConstructors().First(),
                    Parameters.MappingContext,
                    sourceParameter.GetConversionTo(bridge.SourceMember.Type),
                    targetParameter.GetConversionTo(bridge.TargetMember.Type),
                    Parameters.EnumerableIndexNullable,
                    Parameters.ObjectMapperData);

                var constructionLambda = Expression.Lambda<MappingDataCreator<TDeclaredSource, TDeclaredTarget>>(
                    constructorCall,
                    Parameters.MappingContext,
                    sourceParameter,
                    targetParameter,
                    Parameters.EnumerableIndexNullable,
                    Parameters.ObjectMapperData);

                return constructionLambda.Compile();
            });

            return constructionFunc;
        }
    }
}