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

        public static readonly MethodInfo AddChildContextMethod = typeof(IMappingExecutionContext)
            .GetPublicInstanceMethod(nameof(IMappingExecutionContext.AddChild), parameterCount: 6);

        public static readonly MethodInfo AddElementContextMethod = typeof(IMappingExecutionContext)
            .GetPublicInstanceMethod(nameof(IMappingExecutionContext.AddElement), parameterCount: 4);

        public static readonly MethodInfo MapMethod = typeof(IMappingExecutionContext)
            .GetPublicInstanceMethod(nameof(IMappingExecutionContext.Map));

        public static readonly MethodInfo MapRepeatedMethod = typeof(IMappingExecutionContext)
            .GetPublicInstanceMethod(nameof(IMappingExecutionContext.MapRepeated));
    }
}
