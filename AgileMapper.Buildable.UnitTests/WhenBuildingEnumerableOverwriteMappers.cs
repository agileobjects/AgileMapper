namespace AgileObjects.AgileMapper.Buildable.UnitTests
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AgileMapper.UnitTests.Common;
    using AgileMapper.UnitTests.Common.TestClasses;
    using Xunit;
    using GeneratedMapper = Mappers.Mapper;

    public class WhenBuildingEnumerableOverwriteMappers
    {
        [Fact]
        public void ShouldBuildASimpleTypeArrayToArrayMapper()
        {
            var source = new[] { '5', '5', '5' };
            var target = new[] { 3, 3, 3, 3 };

            var result = GeneratedMapper.Map(source).Over(target);

            result.ShouldNotBeNull().ShouldNotBeSameAs(target);
            result.ShouldBe(5, 5, 5);
        }

        [Fact]
        public void ShouldBuildASimpleTypeCollectionToListMapper()
        {
            var source = new Collection<string> { "I", "Will" };
            var target = new List<string> { "You", "Might" };

            var result = GeneratedMapper.Map(source).Over(target);

            result.ShouldNotBeNull().ShouldBeSameAs(target);
            result.SequenceEqual(source).ShouldBeTrue();
        }

        [Fact]
        public void ShouldBuildAComplexTypeArrayToReadOnlyCollectionMapper()
        {
            var source = new[]
            {
                new ProductDto { ProductId = "1", Price = 1.99m },
                new ProductDto { ProductId = "2", Price = 2.99m },
            };

            var target = new ReadOnlyCollection<Product>(new List<Product>
            {
                new Product { ProductId = "2", Price = 4.99 }
            });

            var result = GeneratedMapper.Map(source).Over(target);

            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            result.First().ProductId.ShouldBe("2");
            result.First().Price.ShouldBe(2.99);
            result.Second().ProductId.ShouldBe("1");
            result.Second().Price.ShouldBe(1.99);
        }
    }
}
