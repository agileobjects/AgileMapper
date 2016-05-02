namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingOnToEnumerableMembers
    {
        [Fact]
        public void ShouldMergeAGuidCollection()
        {
            var source = new PublicProperty<ICollection<Guid>>
            {
                Value = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            };

            var target = new PublicField<IEnumerable<Guid>>
            {
                Value = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            };

            var expectedResult = target.Value.Concat(source.Value).ToArray();
            var actualResult = Mapper.Map(source).OnTo(target);

            actualResult.Value.ShouldNotBeNull();
            actualResult.Value.ShouldBeSameAs(target.Value);
            actualResult.Value.SequenceEqual(expectedResult).ShouldBeTrue();
        }

        [Fact]
        public void ShouldMergeAComplexTypeCollection()
        {
            var source = new PublicField<PublicField<long>[]>
            {
                Value = new[] { new PublicField<long> { Value = 456 }, new PublicField<long> { Value = 789 } }
            };

            var target = new PublicField<ICollection<PublicField<int>>>
            {
                Value = new List<PublicField<int>> { new PublicField<int> { Value = 123 } }
            };

            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldBeSameAs(target.Value);
            result.Value.SequenceEqual(r => r.Value, 123, 456, 789).ShouldBeTrue();
        }

        [Fact]
        public void ShouldMergeAnIdentifiableComplexTypeList()
        {
            var source = new PublicProperty<Product[]>
            {
                Value = new[]
                {
                    new Product { ProductId = "Science", Price = 1000.00 }
                }
            };

            var target = new PublicField<IList<Product>>
            {
                Value = new List<Product>
                {
                    new Product { ProductId = "Magic", Price = 1.00 },
                    new Product { ProductId = "Science" }
                }
            };

            var existingProduct = target.Value.Second();
            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldBeSameAs(target.Value);
            result.Value.Second().ShouldBeSameAs(existingProduct);
            result.Value.Second().Price.ShouldBe(1000.00);
            result.Value.SequenceEqual(r => r.ProductId, "Magic", "Science").ShouldBeTrue();
        }

        [Fact]
        public void ShouldHandleANullSourceMember()
        {
            var source = new PublicGetMethod<IEnumerable<byte>>(null);

            var target = new PublicProperty<ICollection<byte>>
            {
                Value = new List<byte> { 1, 2, 4, 8 }
            };

            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeSameAs(target.Value);
        }

        [Fact]
        public void ShouldHandleNoMatchingSourceMember()
        {
            var source = new { HelloThere = "La la la" };

            var target = new PublicProperty<List<DateTime>>
            {
                Value = new List<DateTime> { DateTime.Now, DateTime.UtcNow }
            };

            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldBeSameAs(target.Value);
        }
    }
}
