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

            var namingSettings = mappingContext.MapperContext.NamingSettings;
            var sourceMember = QualifiedMember.From(Member.RootSource(typeof(TDeclaredSource)), namingSettings);
            var targetMember = QualifiedMember.From(Member.RootTarget(typeof(TDeclaredTarget)), namingSettings);

            var bridge = MappingDataFactoryBridge.Create(
                rootInstanceData,
                sourceMember,
                targetMember);

            return Create(bridge);
        }

        public static IObjectMapperCreationData Create<TDeclaredSource, TDeclaredTarget>(
            MappingDataFactoryBridge<TDeclaredSource, TDeclaredTarget> bridge)
        {
            var mapperData = GetObjectMapperData(bridge);

            if (bridge.RuntimeTypesAreTheSame)
            {
                return new MappingData<TDeclaredSource, TDeclaredTarget>(bridge.InstanceData, mapperData);
            }

            var constructionFunc = GetMappingDataCreator(bridge, mapperData);

            return constructionFunc.Invoke(
                bridge.MappingContext,
                bridge.InstanceData.Source,
                bridge.InstanceData.Target,
                bridge.InstanceData.EnumerableIndex,
                mapperData);
        }

        private static ObjectMapperData GetObjectMapperData<TDeclaredSource, TDeclaredTarget>(
            MappingDataFactoryBridge<TDeclaredSource, TDeclaredTarget> bridge)
        {
            var mapperDataKey = DeclaredAndRuntimeTypesKey.ForObjectMapperData(bridge);

            var mapperData = bridge.MappingContext.MapperContext.Cache
                .GetOrAdd(mapperDataKey, k => bridge.GetMapperData());

            return mapperData;
        }

        private static MappingDataCreator<TDeclaredSource, TDeclaredTarget> GetMappingDataCreator<TDeclaredSource, TDeclaredTarget>(
            MappingDataFactoryBridge<TDeclaredSource, TDeclaredTarget> bridge,
            ObjectMapperData mapperData)
        {
            var constructionFunc = GlobalContext.Instance.Cache.GetOrAdd(mapperData, _ =>
            {
                var dataType = typeof(MappingData<,>)
                    .MakeGenericType(bridge.SourceMember.Type, bridge.TargetMember.Type);

                var sourceParameter = Parameters.Create<TDeclaredSource>("source");
                var targetParameter = Parameters.Create<TDeclaredTarget>("target");

                var constructorCall = Expression.New(
                    dataType.GetConstructors(Constants.PublicInstance).First(),
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