namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Reflection;
    using Enumerables;
    using Extensions.Internal;
    using MapperKeys;
    using NetStandardPolyfills;

    internal static class MappingDataFactory
    {
        private static MethodInfo _forChildMethod;
        private static MethodInfo _forElementMethod;

        public static MethodInfo ForChildMethod =>
            _forChildMethod ?? (_forChildMethod = typeof(MappingDataFactory).GetPublicStaticMethod(nameof(ForChild)));

        public static MethodInfo ForElementMethod
            => _forElementMethod ?? (_forElementMethod = typeof(MappingDataFactory).GetPublicStaticMethod(nameof(ForElement)));

        public static ObjectMappingData<TSource, TTarget> ForChild<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? elementIndex,
            object elementKey,
            string targetMemberRegistrationName,
            int dataSourceIndex,
            IObjectMappingDataUntyped parent)
        {
            var mapperKey = new ChildObjectMapperKey(
                MappingTypes<TSource, TTarget>.Fixed,
                targetMemberRegistrationName,
                dataSourceIndex);

            var mappingData = CreateMappingData(source, target, elementIndex, elementKey, mapperKey, parent);

            if (!mappingData.SubMappingNeeded(out var parentMappingData))
            {
                return mappingData;
            }

            var mapperData = parentMappingData.MapperData.ChildMapperDatasOrEmpty.FirstOrDefault(md =>
                (md.DataSourceIndex == dataSourceIndex) &&
                (md.TargetMember.RegistrationName == targetMemberRegistrationName));

            if (mapperData != null)
            {
                mappingData.SetMapper(mapperData.Mapper);
            }

            return mappingData;
        }

        public static ObjectMappingData<TSourceElement, TTargetElement> ForElement<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int elementIndex,
            object elementKey,
            IObjectMappingDataUntyped parent)
        {
            var mapperKey = new ElementObjectMapperKey(MappingTypes<TSourceElement, TTargetElement>.Fixed);

            var mappingData = CreateMappingData(sourceElement, targetElement, elementIndex, elementKey, mapperKey, parent);

            if (mappingData.SubMappingNeeded(out var parentMappingData))
            {
                mappingData.SetMapper(
                    parentMappingData.MapperData.ChildMapperDatas.First().Mapper);
            }

            return mappingData;
        }

        private static ObjectMappingData<TSource, TTarget> CreateMappingData<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? elementIndex,
            object elementKey,
            ObjectMapperKeyBase mapperKey,
            IObjectMappingDataUntyped parent)
        {
            var mappingDataParent = (IObjectMappingData)parent;

            return new ObjectMappingData<TSource, TTarget>(
                source,
                target,
                elementIndex,
                elementKey,
                mapperKey.MappingTypes,
                mappingDataParent.MappingContext,
                mappingDataParent)
            {
                MapperKey = mapperKey
            };
        }

        private static bool SubMappingNeeded(this IObjectMappingData mappingData, out IObjectMappingData parentMappingData)
        {
            parentMappingData = GetParentMappingData(mappingData);

            return parentMappingData.MapperDataPopulated &&
                   parentMappingData.MapperData.Context.NeedsRuntimeTypedMapping;
        }

        private static IObjectMappingData GetParentMappingData(IObjectMappingData mappingData)
        {
            return mappingData.Parent.IsPartOfDerivedTypeMapping
                ? mappingData.Parent.DeclaredTypeMappingData
                : mappingData.Parent;
        }
    }
}