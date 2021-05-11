namespace AgileObjects.AgileMapper.Buildable
{
    using System;
    using System.Reflection;
    using NetStandardPolyfills;

    internal static class BuildableMapperConstants
    {
        public static readonly MethodInfo IsAssignableToMethod = typeof(TypeExtensionsPolyfill)
            .GetPublicStaticMethod(nameof(TypeExtensionsPolyfill.IsAssignableTo));

        public static readonly ConstructorInfo NotSupportedCtor = typeof(NotSupportedException)
            .GetPublicInstanceConstructor(typeof(string));

        public const string MapRepeated = nameof(MapRepeated);
    }
}