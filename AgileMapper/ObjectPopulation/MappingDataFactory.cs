namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq;
    using System.Reflection;
    using Extensions;
    using Members;

    internal static class MappingDataFactory
    {
        public static readonly MethodInfo ForChildObjectMappingDataMethod = typeof(MappingDataFactory)
            .GetPublicStaticMethods()
            .First(m => m.Name == "ForChild" && m.GetParameters().Last().ParameterType == typeof(IObjectMappingData));

        public static InlineChildMappingData<TSource, TTarget> ForChild<TSource, TTarget>(
            TSource source,
            TTarget target,
            int? enumerableIndex,
            string targetMemberRegistrationName,
            int dataSourceIndex,
            IObjectMappingData parent)
        {
            return new InlineChildMappingData<TSource, TTarget>(
                source,
                target,
                enumerableIndex,
                targetMemberRegistrationName,
                dataSourceIndex,
                parent);
        }

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
    }
}