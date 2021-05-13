namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
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

        [Fact]
        public void ShouldBuildAComplexTypeListToIListMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<List<ProductDto>>().ToANew<IList<ProductDto>>(new MappingPlanSettings
                {
                    LazyCompile = true
                });

                var sourceCodeExpressions = mapper.GetPlanSourceCodeInCache();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new List<ProductDto>
                {
                    new ProductDto { ProductId = "Surprise" },
                    null,
                    new ProductDto { ProductId = "Boomstick" }
                };

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                var result = executor
                    .ShouldHaveACreateNewMethod()
                    .ShouldExecuteACreateNewMapping<List<ProductDto>>(executor);

                result.ShouldNotBeNull();
                result.ShouldNotBeSameAs(source);
                result.First().ShouldNotBeNull().ProductId.ShouldBe("Surprise");
                result.Second().ShouldBeNull();
                result.Third().ShouldNotBeNull().ProductId.ShouldBe("Boomstick");
            }
        }
    }
}
