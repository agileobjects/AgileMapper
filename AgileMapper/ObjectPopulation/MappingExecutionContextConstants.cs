namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Reflection;
    using NetStandardPolyfills;

    internal static class MappingExecutionContextConstants
    {
        public static readonly MethodInfo RegisterMethod = typeof(IMappingExecutionContext)
            .GetPublicInstanceMethod(nameof(IMappingExecutionContext.Register));

        public static readonly MethodInfo TryGetMethod = typeof(IMappingExecutionContext)
            .GetPublicInstanceMethod(nameof(IMappingExecutionContext.TryGet));

        public static readonly MethodInfo MapChildMethod =
            typeof(IMappingExecutionContext)
                .GetPublicInstanceMethod("Map", parameterCount: 5);

        public static readonly MethodInfo MapElementMethod =
            typeof(IMappingExecutionContext)
                .GetPublicInstanceMethod("Map", parameterCount: 7);

        public static readonly MethodInfo MapRepeatedChildMethod = 
            typeof(IMappingExecutionContext)
                .GetPublicInstanceMethod("MapRepeated", parameterCount: 7);

        public static readonly MethodInfo MapRepeatedElementMethod = 
            typeof(IMappingExecutionContext)
                .GetPublicInstanceMethod("MapRepeated", parameterCount: 5);
    }
}
