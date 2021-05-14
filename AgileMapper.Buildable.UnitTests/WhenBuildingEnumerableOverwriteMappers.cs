namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Xunit;

    public class WhenBuildingEnumerableOverwriteMappers
    {
        [Fact]
        public void ShouldBuildASimpleTypeArrayToArrayMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<char[]>().Over<int[]>();

                var sourceCodeExpressions = mapper.GetPlanSourceCodeInCache();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new[] { '5', '5', '5' };
                var target = new[] { 3, 3, 3, 3 };

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                var result = executor
                    .ShouldHaveAnOverwriteMethod()
                    .ShouldExecuteAnOverwriteMapping(target);

                result.ShouldNotBeNull().ShouldNotBeSameAs(target);
                result.ShouldBe(5, 5, 5);
            }
        }

        [Fact]
        public void ShouldBuildASimpleTypeCollectionToListMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<Collection<string>>().Over<List<string>>();

                var sourceCodeExpressions = mapper.GetPlanSourceCodeInCache();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new Collection<string> { "I", "Will" };
                var target = new List<string> { "You", "Might" };

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                var result = executor
                    .ShouldHaveAnOverwriteMethod()
                    .ShouldExecuteAnOverwriteMapping(target);

                result.ShouldNotBeNull().ShouldBeSameAs(target);
                result.SequenceEqual(source).ShouldBeTrue();
            }
        }

        [Fact]
        public void ShouldBuildAComplexTypeArrayToReadOnlyCollectionMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.GetPlanFor<ProductDto[]>().Over<ReadOnlyCollection<Product>>();

                var sourceCodeExpressions = mapper.GetPlanSourceCodeInCache();

                var staticMapperClass = sourceCodeExpressions
                    .ShouldCompileAStaticMapperClass();

                var staticMapMethod = staticMapperClass
                    .GetMapMethods()
                    .ShouldHaveSingleItem();

                var source = new[]
                {
                    new ProductDto { ProductId = "1", Price = 1.99m },
                    new ProductDto { ProductId = "2", Price = 2.99m },
                };

                var target = new ReadOnlyCollection<Product>(new List<Product>
                {
                    new Product { ProductId = "2", Price = 4.99 }
                });

                var executor = staticMapMethod
                    .ShouldCreateMappingExecutor(source);

                var result = executor
                    .ShouldHaveAnOverwriteMethod()
                    .ShouldExecuteAnOverwriteMapping(target);

                result.ShouldNotBeNull();
                result.Count.ShouldBe(2);
                result.First().ProductId.ShouldBe("2");
                result.First().Price.ShouldBe(2.99);
                result.Second().ProductId.ShouldBe("1");
                result.Second().Price.ShouldBe(1.99);
            }
        }
    }
}
