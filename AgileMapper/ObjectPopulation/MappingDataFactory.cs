namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Reflection;
    using Extensions;
    using Members;

    internal static class MappingDataFactory
    {
        public static readonly MethodInfo ForChildMethod = typeof(MappingDataFactory)
            .GetPublicStaticMethod("ForChild");

        public static readonly MethodInfo ForElementMethod = typeof(MappingDataFactory)
            .GetPublicStaticMethod("ForElement");

        public static InlineChildMappingData<TSource, TTarget> ForChild<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            string targetMemberRegistrationName,
            int dataSourceIndex,
            IInlineMappingData parent)
        {
            return new InlineChildMappingData<TSource, TTarget>(
                source,
                target,
                enumerableIndex,
                targetMemberRegistrationName,
                dataSourceIndex,
                parent);
        }

        public static InlineElementMappingData<TSourceElement, TTargetElement> ForElement<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int enumerableIndex,
            IInlineMappingData parent)
        {
            return new InlineElementMappingData<TSourceElement, TTargetElement>(
                sourceElement,
                targetElement,
                enumerableIndex,
                parent);
        }
    }
}