namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Reflection;
    using Extensions;
    using Members;
    using Members.Sources;

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
            var mapperKey = new RootObjectMapperKey(mappingContext.RuleSet, MappingTypes.Fixed<TSource, TTarget>());

            return new ObjectMappingData<TSource, TTarget>(
                source,
                target,
                null,
                mapperKey,
                mappingContext.MapperContext.RootMembersSource,
                mappingContext,
                parent: null);
        }

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

            var membersSource = new MemberLookupsChildMembersSource(parent, targetMemberRegistrationName, dataSourceIndex);

            return new ObjectMappingData<TSource, TTarget>(
                source,
                target,
                enumerableIndex,
                mapperKey,
                membersSource,
                parent.MappingContext,
                parent);
        }

        public static ObjectMappingData<TSourceElement, TTargetElement> ForElement<TSourceElement, TTargetElement>(
            TSourceElement sourceElement,
            TTargetElement targetElement,
            int enumerableIndex,
            IObjectMappingData parent)
        {
            return new ObjectMappingData<TSourceElement, TTargetElement>(
                sourceElement,
                targetElement,
                enumerableIndex,
                new ElementObjectMapperKey(MappingTypes.Fixed<TSourceElement, TTargetElement>()),
                new ElementMembersSource(parent),
                parent.MappingContext,
                parent);
        }
    }
}