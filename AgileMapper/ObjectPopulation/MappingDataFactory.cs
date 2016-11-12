namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Reflection;
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

            var mappingDataParent = (IObjectMappingData)parent;

            return new ObjectMappingData<TSource, TTarget>(
                source,
                target,
                enumerableIndex,
                mapperKey,
                mappingDataParent.MappingContext,
                mappingDataParent);
        }

        public static ObjectMappingData<TSourceElement, TTargetElement> ForElement<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int enumerableIndex,
            IObjectMappingDataUntyped parent)
        {
            var mapperKey = new ElementObjectMapperKey(MappingTypes.Fixed<TSourceElement, TTargetElement>());

            var mappingDataParent = (IObjectMappingData)parent;

            return new ObjectMappingData<TSourceElement, TTargetElement>(
                sourceElement,
                targetElement,
                enumerableIndex,
                mapperKey,
                mappingDataParent.MappingContext,
                mappingDataParent);
        }
    }
}