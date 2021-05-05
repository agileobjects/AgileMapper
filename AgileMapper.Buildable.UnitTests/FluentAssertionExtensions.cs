namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using AgileMapper.UnitTests.Common;
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

        public static MethodInfo ShouldHaveACreateNewMethod<TSource>(
            this MappingExecutor<TSource> executor)
        {
            return executor.GetType()
                .GetPublicInstanceMethods("ToANew")
                .ShouldHaveSingleItem();
        }

        public static MethodInfo ShouldHaveAMergeMethod<TSource>(
            this MappingExecutor<TSource> executor)
        {
            return executor.GetType()
                .GetPublicInstanceMethods("OnTo")
                .ShouldHaveSingleItem();
        }

        public static MethodInfo ShouldHaveAnOverwriteMethod<TSource>(
            this MappingExecutor<TSource> executor)
        {
            return executor.GetType()
                .GetPublicInstanceMethods("Over")
                .ShouldHaveSingleItem();
        }

        public static TResult ShouldExecuteACreateNewMapping<TResult>(
            this MethodInfo createNewMethod,
            object executor)
        {
            return createNewMethod
                .MakeGenericMethod(typeof(TResult))
                .Invoke(executor, Array.Empty<object>())
                .ShouldNotBeNull()
                .ShouldBeOfType<TResult>();
        }

        public static TTarget ShouldExecuteAMergeMapping<TTarget>(
            this MethodInfo mergeMethod,
            object executor,
            TTarget target)
        {
            return mergeMethod
                .Invoke(executor, new object[] { target })
                .ShouldNotBeNull()
                .ShouldBeSameAs(target)
                .ShouldBeOfType<TTarget>();
        }

        public static TTarget ShouldExecuteAnOverwriteMapping<TTarget>(
            this MethodInfo overwriteMethod,
            object executor,
            TTarget target)
        {
            return overwriteMethod
                .Invoke(executor, new object[] { target })
                .ShouldNotBeNull()
                .ShouldBeSameAs(target)
                .ShouldBeOfType<TTarget>();
        }
    }
}
