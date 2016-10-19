namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq;
    using System.Reflection;
    using Extensions;
    using Members;

    internal static class MappingDataFactory
    {
        public static readonly MethodInfo ForChildMethod = typeof(MappingDataFactory)
            .GetPublicStaticMethod("ForChild");

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