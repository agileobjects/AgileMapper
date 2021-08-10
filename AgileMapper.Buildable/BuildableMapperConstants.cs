namespace AgileObjects.AgileMapper.Buildable
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using NetStandardPolyfills;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;

    internal static class BuildableMapperConstants
    {
        public static readonly MethodInfo IsAssignableToMethod = typeof(TypeExtensionsPolyfill)
            .GetPublicStaticMethod(nameof(TypeExtensionsPolyfill.IsAssignableTo));

        public static readonly ConstructorInfo NotSupportedCtor = typeof(NotSupportedException)
            .GetPublicInstanceConstructor(typeof(string));

        public static readonly MethodInfo StringConcatMethod = typeof(string).GetPublicStaticMethod(
            nameof(string.Concat),
            typeof(string),
            typeof(string),
            typeof(string));

        public static readonly Expression NullConfiguration =
            Expression.Default(typeof(Func<ITranslationSettings, ITranslationSettings>));

        public static readonly MethodInfo GetFriendlyNameMethod = typeof(PublicTypeExtensions)
            .GetPublicStaticMethod(
                nameof(PublicTypeExtensions.GetFriendlyName),
                typeof(Type),
                NullConfiguration.Type);

        public const string MapRepeated = nameof(MapRepeated);
    }
}