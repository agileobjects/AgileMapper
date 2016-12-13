namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq;
    using System.Reflection;
    using Enumerables;
    using Extensions;
    using Members;
    using NetStandardPolyfills;

    internal static class MappingDataFactory
    {
        public static readonly MethodInfo ForChildMethod = typeof(MappingDataFactory)
            .GetPublicStaticMethod("ForChild");

        public static readonly MethodInfo ForElementMethod = typeof(MappingDataFactory)
            .GetPublicStaticMethod("ForElement");

        public static ObjectMappingData<TSource, TTarget> ForChild<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            string targetMemberRegistrationName,
            int dataSourceIndex,
            IObjectMappingDataUntyped parent)
        {
            var mapperKey = new ChildObjectMapperKey(
                targetMemberRegistrationName,
                dataSourceIndex,
                MappingTypes.Fixed<TSource, TTarget>());

            var mappingData = CreateMappingData(source, target, enumerableIndex, mapperKey, parent);

            var parentMappingData = GetParentMappingData(mappingData);

            mappingData.MapperData = parentMappingData.MapperData.ChildMapperDatas.HasOne()
                ? parentMappingData.MapperData.ChildMapperDatas.First()
                : parentMappingData.MapperData.ChildMapperDatas.First(md =>
                    (md.DataSourceIndex == dataSourceIndex) &&
                    (md.TargetMember.RegistrationName == targetMemberRegistrationName));

            return mappingData;
        }

        public static ObjectMappingData<TSourceElement, TTargetElement> ForElement<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int enumerableIndex,
            IObjectMappingDataUntyped parent)
        {
            var mapperKey = new ElementObjectMapperKey(MappingTypes.Fixed<TSourceElement, TTargetElement>());

            var mappingData = CreateMappingData(sourceElement, targetElement, enumerableIndex, mapperKey, parent);

            mappingData.MapperData = GetParentMappingData(mappingData).MapperData.ChildMapperDatas.First();

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
                mapperKey,
                mappingDataParent.MappingContext,
                mappingDataParent);
        }

        private static IObjectMappingData GetParentMappingData(IObjectMappingData mappingData)
        {
            return mappingData.Parent.IsPartOfDerivedTypeMapping
                ? mappingData.Parent.DeclaredTypeMappingData
                : mappingData.Parent;
        }
    }
}