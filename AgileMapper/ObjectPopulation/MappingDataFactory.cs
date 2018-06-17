namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Reflection;
    using Enumerables;
    using Extensions.Internal;
    using MapperKeys;
    using Members;
    using NetStandardPolyfills;

    internal static class MappingDataFactory
    {
        private static MethodInfo _forChildMethod;
        private static MethodInfo _forElementMethod;

        public static MethodInfo ForChildMethod =>
            _forChildMethod ?? (_forChildMethod = typeof(MappingDataFactory).GetPublicStaticMethod("ForChild"));

        public static MethodInfo ForElementMethod
            => _forElementMethod ?? (_forElementMethod = typeof(MappingDataFactory).GetPublicStaticMethod("ForElement"));

        public static ObjectMappingData<TSource, TTarget> ForChild<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            string targetMemberRegistrationName,
            int dataSourceIndex,
            IObjectMappingDataUntyped parent)
        {
            var mapperKey = new ChildObjectMapperKey(
                MappingTypes<TSource, TTarget>.Fixed,
                targetMemberRegistrationName,
                dataSourceIndex);

            var mappingData = CreateMappingData(source, target, enumerableIndex, mapperKey, parent);

            if (!ChildMappersNeeded(mappingData.Parent))
            {
                return mappingData;
            }

            var parentMappingData = GetParentMappingData(mappingData);

            var mapperData = parentMappingData.MapperData.ChildMapperDatas.HasOne()
                ? parentMappingData.MapperData.ChildMapperDatas.First()
                : parentMappingData.MapperData.ChildMapperDatas.First(md =>
                    (md.DataSourceIndex == dataSourceIndex) &&
                    (md.TargetMember.RegistrationName == targetMemberRegistrationName));

            mappingData.SetMapper(mapperData.Mapper);

            return mappingData;
        }

        public static ObjectMappingData<TSourceElement, TTargetElement> ForElement<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int enumerableIndex,
            IObjectMappingDataUntyped parent)
        {
            var mapperKey = new ElementObjectMapperKey(MappingTypes<TSourceElement, TTargetElement>.Fixed);

            var mappingData = CreateMappingData(sourceElement, targetElement, enumerableIndex, mapperKey, parent);

            if (ChildMappersNeeded(mappingData.Parent))
            {
                mappingData.SetMapper(
                    GetParentMappingData(mappingData).MapperData.ChildMapperDatas.First().Mapper);
            }

            return mappingData;
        }

        private static ObjectMappingData<TSource, TTarget> CreateMappingData<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            ObjectMapperKeyBase mapperKey,
            IObjectMappingDataUntyped parent)
        {
            var mappingDataParent = (IObjectMappingData)parent;

            return new ObjectMappingData<TSource, TTarget>(
                source,
                target,
                enumerableIndex,
                mapperKey.MappingTypes,
                mappingDataParent.MappingContext,
                mappingDataParent)
            {
                MapperKey = mapperKey
            };
        }

        private static bool ChildMappersNeeded(IObjectMappingData mappingData)
        {
            while (!mappingData.IsStandalone())
            {
                mappingData = mappingData.Parent;
            }

            return mappingData.MapperData.Context.NeedsSubMapping;
        }

        private static IObjectMappingData GetParentMappingData(IObjectMappingData mappingData)
        {
            return mappingData.Parent.IsPartOfDerivedTypeMapping
                ? mappingData.Parent.DeclaredTypeMappingData
                : mappingData.Parent;
        }
    }
}