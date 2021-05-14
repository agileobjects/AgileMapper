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

        public static MappingExecutionContextBase<TSource> ShouldCreateMappingExecutor<TSource>(
            this MethodInfo staticMapMethod,
            TSource source)
        {
            return staticMapMethod
                .Invoke(null, new object[] { source })
                .ShouldNotBeNull()
                .ShouldBeOfType<MappingExecutionContextBase<TSource>>();
        }

        public static ExecutorTester<TSource> ShouldHaveACreateNewMethod<TSource>(
            this MappingExecutionContextBase<TSource> executor)
        {
            return new ExecutorTester<TSource>(
                executor,
                executor.GetType()
                    .GetPublicInstanceMethods("ToANew")
                    .ShouldHaveSingleItem());
        }

        public static ExecutorTester<TSource> ShouldHaveAMergeMethod<TSource>(
            this MappingExecutionContextBase<TSource> executor)
        {
            return new ExecutorTester<TSource>(
                executor,
                executor.GetType()
                    .GetPublicInstanceMethods("OnTo")
                    .ShouldHaveSingleItem());
        }

        public static ExecutorTester<TSource> ShouldHaveAnOverwriteMethod<TSource>(
            this MappingExecutionContextBase<TSource> executor)
        {
            return new ExecutorTester<TSource>(
                executor,
                executor.GetType()
                    .GetPublicInstanceMethods("Over")
                    .ShouldHaveSingleItem());
        }

        public class ExecutorTester<TSource>
        {
            private readonly MappingExecutionContextBase<TSource> _executor;
            private readonly MethodInfo _mappingMethod;

            public ExecutorTester(
                MappingExecutionContextBase<TSource> executor,
                MethodInfo mappingMethod)
            {
                _executor = executor;
                _mappingMethod = mappingMethod;
            }

            public TResult ShouldExecuteACreateNewMapping<TResult>()
            {
                return _mappingMethod
                    .MakeGenericMethod(typeof(TResult))
                    .Invoke(_executor, Array.Empty<object>())
                    .ShouldNotBeNull()
                    .ShouldBeOfType<TResult>();
            }

            public TTarget ShouldExecuteAMergeMapping<TTarget>(TTarget target)
            {
                return _mappingMethod
                    .Invoke(_executor, new object[] { target })
                    .ShouldNotBeNull()
                    .ShouldBeSameAs(target)
                    .ShouldBeOfType<TTarget>();
            }

            public TTarget ShouldExecuteAnOverwriteMapping<TTarget>(TTarget target)
            {
                return _mappingMethod
                    .Invoke(_executor, new object[] { target })
                    .ShouldNotBeNull()
                    .ShouldBeSameAs(target)
                    .ShouldBeOfType<TTarget>();
            }
        }
    }
}
