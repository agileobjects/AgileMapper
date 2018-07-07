namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
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
        public void ShouldMergeANullableIntArray()
        {
            var source = new PublicProperty<ICollection<long>>
            {
                Value = new List<long> { 2, 3 }
            };

            var target = new PublicField<IList<int?>>
            {
                Value = new int?[] { 1, 2, null }
            };

            var originalArray = target.Value;
            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldNotBeSameAs(originalArray);
            result.Value.ShouldBe(1, 2, null, 3);
        }

        [Fact]
        public void ShouldMergeANullableGuidReadOnlyCollection()
        {
            var source = new PublicProperty<ICollection<Guid>>
            {
                Value = new List<Guid> { Guid.NewGuid() }
            };

            var target = new PublicField<ReadOnlyCollection<Guid?>>
            {
                Value = new ReadOnlyCollection<Guid?>(new[] { Guid.Empty, default(Guid?), Guid.NewGuid() })
            };

            var originalCollection = target.Value;
            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldNotBeSameAs(originalCollection);
            result.Value.ShouldBe(target.Value.First(), default(Guid?), target.Value.Third(), source.Value.First());
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
            result.Value.ShouldBe(r => r.Value, 123, 456, 789);
        }

#if !NET35
        [Fact]
        public void ShouldMergeAComplexTypeIReadOnlyCollectionList()
        {
            var source = new PublicField<IReadOnlyCollection<PublicField<long>>>
            {
                Value = new[] { new PublicField<long> { Value = 456 }, new PublicField<long> { Value = 789 } }
            };

            var target = new PublicField<IReadOnlyCollection<PublicField<int>>>
            {
                Value = new List<PublicField<int>> { new PublicField<int> { Value = 123 } }
            };

            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldBeSameAs(target.Value);
            result.Value.ShouldBe(r => r.Value, 123, 456, 789);
        }
#endif
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
            result.Value.ShouldBe(r => r.ProductId, "Magic", "Science");
        }

        [Fact]
        public void ShouldMergeAnIdentifiableComplexTypeReadOnlyCollection()
        {
            var source = new PublicProperty<Product[]>
            {
                Value = new[]
                {
                    new Product { ProductId = "Science", Price = 1000.00 }
                }
            };

            var target = new PublicField<ReadOnlyCollection<Product>>
            {
                Value = new ReadOnlyCollection<Product>(new List<Product>
                {
                    new Product { ProductId = "Science" },
                    new Product { ProductId = "Magic", Price = 1.00 }
                })
            };

            var existingProduct = target.Value.First();
            var result = Mapper.Map(source).OnTo(target);

            result.Value.First().ShouldBeSameAs(existingProduct);
            result.Value.First().Price.ShouldBe(1000.00);
            result.Value.ShouldBe(r => r.ProductId, "Science", "Magic");
        }

#if !NET35
        [Fact]
        public void ShouldMergeAnIdentifiableComplexTypeIReadOnlyCollectionArray()
        {
            var source = new PublicProperty<ProductDto[]>
            {
                Value = new[]
                {
                    new ProductDto { ProductId = "Magic", Price = 1.00m }
                }
            };

            var target = new PublicField<IReadOnlyCollection<Product>>
            {
                Value = new[]
                {
                    new Product { ProductId = "Magic" },
                    new Product { ProductId = "Science", Price = 1000.00 }
                }
            };

            var existingProduct = target.Value.First();
            var result = Mapper.Map(source).OnTo(target);

            result.Value.First().ShouldBeSameAs(existingProduct);
            result.Value.First().Price.ShouldBe(1.00);
            result.Value.Second().Price.ShouldBe(1000.00);
            result.Value.ShouldBe(r => r.ProductId, "Magic", "Science");
        }
#endif
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
        public void ShouldHandleANullTargetCollection()
        {
            var source = new PublicField<IList<byte>>
            {
                Value = new List<byte> { 2, 4, 6, 8 }
            };

            var target = new PublicProperty<ICollection<int>> { Value = null };

            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBe(2, 4, 6, 8);
        }

        [Fact]
        public void ShouldHandleANullIdentifiableTargetEnumerable()
        {
            var source = new PublicField<IList<PersonViewModel>>
            {
                Value = new List<PersonViewModel>
                {
                    new PersonViewModel { Id = Guid.NewGuid() }
                }
            };

            var target = new PublicField<IEnumerable<Person>> { Value = null };

            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldNotBeNull();
            result.Value.ShouldHaveSingleItem();
            result.Value.First().Id.ShouldBe(source.Value.First().Id);
        }

#if !NET35
        [Fact]
        public void ShouldHandleANullIReadOnlyCollection()
        {
            var source = new PublicField<IList<string>>
            {
                Value = new[] { "X", "Y", "Z" }
            };

            var target = new PublicProperty<IReadOnlyCollection<char>> { Value = null };

            var result = Mapper.Map(source).OnTo(target);

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBe('X', 'Y', 'Z');
        }
#endif
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

        [Fact]
        public void ShouldHandleANonEnumerableSourceMember()
        {
            var source = new PublicField<Person>
            {
                Value = new Person { Name = "Error" }
            };

            var target = new PublicField<IEnumerable<Product>>
            {
                Value = new List<Product>
                {
                    new Product { ProductId = "Swing", Price = 9.99 }
                }
            };

            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target);
        }
    }
}
