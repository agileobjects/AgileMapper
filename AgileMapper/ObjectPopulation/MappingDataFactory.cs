namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Reflection;
    using Extensions;
    using Members;

    internal static class MappingDataFactory
    {
        public static readonly MethodInfo ForRootMethod = typeof(MappingDataFactory)
            .GetPublicStaticMethod("ForRoot");

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
            IObjectMappingData parent)
        {
            var mapperKey = new ChildObjectMapperKey(
                targetMemberRegistrationName,
                dataSourceIndex,
                MappingTypes.Fixed<TSource, TTarget>());

            return new ObjectMappingData<TSource, TTarget>(
                source,
                target,
                enumerableIndex,
                mapperKey,
                parent.MappingContext,
                parent);
        }

        public static ObjectMappingData<TSourceElement, TTargetElement> ForElement<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int enumerableIndex,
            IObjectMappingData parent)
        {
            var mapperKey = new ElementObjectMapperKey(MappingTypes.Fixed<TSourceElement, TTargetElement>());

            return new ObjectMappingData<TSourceElement, TTargetElement>(
                sourceElement,
                targetElement,
                enumerableIndex,
                mapperKey,
                parent.MappingContext,
                parent);
        }
    }
}