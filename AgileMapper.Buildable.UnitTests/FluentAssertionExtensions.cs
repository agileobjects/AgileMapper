namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using BuildableExpressions.Compilation;
    using BuildableExpressions.SourceCode;
    using NetStandardPolyfills;

    internal static class FluentAssertionExtensions
    {
        public static Type ShouldCompileAStaticMapperClass(
            this IEnumerable<SourceCodeExpression> sourceCodeExpressions)
        {
            var mapperAssembly = sourceCodeExpressions
                .ShouldHaveSingleItem()
                .Compile()
                .CompiledAssembly
                .ShouldNotBeNull();

            var staticMapperClass = mapperAssembly
                .GetType("AgileObjects.AgileMapper.Buildable." + nameof(Mapper))
                .ShouldNotBeNull();

            return staticMapperClass;
        }

        public static IEnumerable<MethodInfo> GetMapMethods(this Type staticMapperClass)
            => staticMapperClass.GetPublicStaticMethods("Map");

        public static MappingExecutor<TSource> ShouldCreateMappingExecutor<TSource>(
            this MethodInfo staticMapMethod,
            TSource source)
        {
            return staticMapMethod
                .Invoke(null, new object[] { source })
                .ShouldNotBeNull()
                .ShouldBeOfType<MappingExecutor<TSource>>();
        }

        public static MethodInfo ShouldHaveAToANewMethod<TSource>(
            this MappingExecutor<TSource> executor)
        {
            return executor.GetType()
                .GetPublicInstanceMethods("ToANew")
                .ShouldHaveSingleItem();
        }

        public static TResult ShouldExecuteAToANewMapping<TResult>(
            this MethodInfo createNewMethod,
            object executor)
        {
            return createNewMethod
                .MakeGenericMethod(typeof(TResult))
                .Invoke(executor, Array.Empty<object>())
                .ShouldNotBeNull()
                .ShouldBeOfType<TResult>();
        }
    }
}
