namespace AgileObjects.AgileMapper.ObjectPopulation
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;

    internal static class MappingExecutionContextConstants
    {
        private static ParameterExpression _parameter;

        public static ParameterExpression Parameter
            => _parameter ??= typeof(MappingExecutionContextBase2).GetOrCreateParameter("ctx");

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
