namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using AgileMapper.UnitTests.Common;
    using Plans;
    using Xunit;

    public class WhenBuildingEnumerableCreateNewMappers
    {
        [Fact]
        public void ShouldBuildASimpleTypeListToCollectionMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<List<string>>().ToANew<Collection<byte?>>(new MappingPlanSettings
                {
                    LazyCompile = true
                });

                var sourceCodeExpressions = mapper.GetPlanSourceCodeInCache();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new List<string> { "3", "2", "1", "12345" };

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                var result = executor
                    .ShouldHaveACreateNewMethod()
                    .ShouldExecuteACreateNewMapping<Collection<byte?>>(executor);

                result.ShouldNotBeNull();
                result.ShouldBe<byte?>(3, 2, 1, null);
            }
        }

        [Fact]
        public void ShouldBuildASimpleTypeArrayToReadOnlyCollectionMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<int[]>().ToANew<ReadOnlyCollection<int>>(new MappingPlanSettings
                {
                    LazyCompile = true
                });

                var sourceCodeExpressions = mapper.GetPlanSourceCodeInCache();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new[] { 1, 2, 3 };

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                var result = executor
                    .ShouldHaveACreateNewMethod()
                    .ShouldExecuteACreateNewMapping<ReadOnlyCollection<int>>(executor);

                result.ShouldNotBeNull();
                result.ShouldBe(1, 2, 3);
            }
        }

        [Fact]
        public void ShouldBuildASimpleTypeHashSetToArrayMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<HashSet<DateTime>>().ToANew<DateTime[]>(new MappingPlanSettings
                {
                    LazyCompile = true
                });

                var sourceCodeExpressions = mapper.GetPlanSourceCodeInCache();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var today = DateTime.Today;
                var tomorrow = today.AddDays(+1);
                var yesterday = today.AddDays(-1);

                var source = new HashSet<DateTime> { yesterday, today, tomorrow };

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                var result = executor
                    .ShouldHaveACreateNewMethod()
                    .ShouldExecuteACreateNewMapping<DateTime[]>(executor);

                result.ShouldNotBeNull().ShouldBe(yesterday, today, tomorrow);
            }
        }
    }
}
