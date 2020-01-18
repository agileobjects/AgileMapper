#if FEATURE_DYNAMIC_ROOT_SOURCE
namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic;
    using System.Linq;
    using Common;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDynamicsOverEnumerableMembers
    {
        [Fact]
        public void ShouldOverwriteASimpleTypeCollection()
        {
            dynamic source = new ExpandoObject();
            source.Value = new List<int> { 10, 20, 30 };

            var target = new PublicField<ICollection<string>>
            {
                Value = new List<string> { "40" }
            };

            Mapper.Map(source).Over(target);

            target.Value.ShouldBe("10", "20", "30");
        }

        [Fact]
        public void ShouldOverwriteAComplexTypeCollectionToEmpty()
        {
            dynamic source = new ExpandoObject();
            source.Value = default(List<ProductDto>);

            var target = new PublicField<ICollection<ProductDto>>
            {
                Value = new List<ProductDto> { new ProductDto { ProductId = "p-1" } }
            };

            Mapper.Map(source).Over(target);

            target.Value.ShouldBeEmpty();
        }

        [Fact]
        public void ShouldOverwriteAnIndentifableComplexTypeCollection()
        {
            dynamic source = new ExpandoObject();

            source.Value = new List<ProductDto>
            {
                new ProductDto { ProductId = "prod-1", Price = 12.99m },
                new ProductDto { ProductId = "prod-2", Price = 15.00m },
            };

            var target = new PublicField<ICollection<Product>>
            {
                Value = new List<Product>
                {
                    new Product { ProductId = "prod-2", Price = 20.00 },
                    new Product { ProductId = "prod-3", Price =  1.99 },
                }
            };

            var preMappingProd2 = target.Value.First();

            Mapper.Map(source).Over(target);

            target.Value.Count.ShouldBe(2);

            target.Value.First().ShouldBeSameAs(preMappingProd2);
            target.Value.ShouldBe(p => p.ProductId, "prod-2", "prod-1");
            target.Value.ShouldBe(p => p.Price, 15.00, 12.99);
        }

        [Fact]
        public void ShouldOverwriteAComplexTypeCollectionFromElementEntries()
        {
            dynamic source = new ExpandoObject();
            source.Value_0 = new PublicField<string> { Value = "Value 0" };
            source.Value_1 = new PublicField<string> { Value = "Value 1" };

            var target = new PublicField<Collection<PublicField<string>>>
            {
                Value = new Collection<PublicField<string>>
                {
                    new PublicField<string> { Value = "Value 1" },
                    new PublicField<string> { Value = "Value 2" },
                }
            };

            Mapper.Map(source).Over(target);

            target.Value.Count.ShouldBe(2);
            target.Value.ShouldBe(pf => pf.Value, "Value 0", "Value 1");
        }
    }
}
#endif