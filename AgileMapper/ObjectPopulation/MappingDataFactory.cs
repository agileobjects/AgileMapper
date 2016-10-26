namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Reflection;
    using Extensions;

    internal static class MappingDataFactory
    {
        public static readonly MethodInfo ForRootMethod = typeof(MappingDataFactory)
            .GetPublicStaticMethod("ForRoot");

        public static readonly MethodInfo ForChildMethod = typeof(MappingDataFactory)
            .GetPublicStaticMethod("ForChild");

        public static readonly MethodInfo ForElementMethod = typeof(MappingDataFactory)
            .GetPublicStaticMethod("ForElement");

        public static ObjectMappingData<TSource, TTarget> ForRoot<TSource, TTarget>(
            TSource source,
            TTarget target,
            IMappingContext mappingContext)
        {
            return (ObjectMappingData<TSource, TTarget>)ObjectMappingDataFactory.ForRoot(
                source,
                target,
                mappingContext);
        }

        public static ObjectMappingData<TSource, TTarget> ForChild<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            string targetMemberRegistrationName,
            int dataSourceIndex,
            IObjectMappingData parent)
        {
            return (ObjectMappingData<TSource, TTarget>)ObjectMappingDataFactory.ForChild(
                source,
                target,
                enumerableIndex,
                targetMemberRegistrationName,
                dataSourceIndex,
                parent);
        }

        public static ObjectMappingData<TSourceElement, TTargetElement> ForElement<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int enumerableIndex,
            IObjectMappingData parent)
        {
            return (ObjectMappingData<TSourceElement, TTargetElement>)ObjectMappingDataFactory.ForElement(
                sourceElement,
                targetElement,
                enumerableIndex,
                parent);
        }
    }
}