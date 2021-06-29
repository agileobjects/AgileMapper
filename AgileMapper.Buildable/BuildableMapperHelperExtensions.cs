namespace AgileObjects.AgileMapper.Buildable
{
    using System;
    using BuildableExpressions.SourceCode;
    using NetStandardPolyfills;

    internal static class BuildableMapperHelperExtensions
    {
        public static Type GetTargetType(this MethodExpression mapMethod)
            => mapMethod.Parameters[0].Type.GetGenericTypeArguments()[1];
    }
}