namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using TestClasses;
    using static System.Decimal;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingOverEnumerableMembers
    {
        [Fact]
        public void ShouldOverwriteAndConvertACollection()
        {
            var source = new PublicProperty<IEnumerable<string>>
            {
                Value = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() }
            };

            var target = new PublicField<ICollection<Guid>>
            {
                Value = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
            };

            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeSameAs(target.Value);
            result.Value.ShouldBe(source.Value, r => r.ToString());
        }

        [Fact]
        public void ShouldOverwriteAnArray()
        {
            var source = new PublicProperty<IEnumerable<decimal>>
            {
                Value = new[] { MinusOne, Zero }
            };

            var target = new PublicField<decimal[]>
            {
                Value = new[] { MinValue, MaxValue }
            };

            var originalTargetArray = target.Value;
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldNotBeSameAs(source.Value);
            result.Value.ShouldNotBeSameAs(originalTargetArray);
            result.Value.ShouldBe(MinusOne, Zero);
        }

        [Fact]
        public void ShouldOverwriteAReadOnlyCollection()
        {
            var source = new PublicProperty<ICollection<decimal>>
            {
                Value = new[] { MinValue, MaxValue }
            };

            var target = new PublicField<ReadOnlyCollection<decimal>>
            {
                Value = new ReadOnlyCollection<decimal>(new[] { MinusOne, Zero })
            };

            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBe(MinValue, MaxValue);
        }

#if !NET35
        [Fact]
        public void ShouldOverwriteAnIReadOnlyCollectionCollection()
        {
            var source = new PublicProperty<IEnumerable<decimal>>
            {
                Value = new[] { MinusOne, Zero }
            };

            var target = new PublicField<IReadOnlyCollection<decimal>>
            {
                Value = new Collection<decimal>(new[] { MinValue, MaxValue })
            };

            var preMappingCollection = target.Value;

            var result = Mapper.Map(source).Over(target);

            // (Collection<decimal> as ICollection<decimal>).IsReadOnly == true, so the 
            // Collection does not get reused:
            result.Value.ShouldNotBeSameAs(preMappingCollection);
            result.Value.ShouldBe(MinusOne, Zero);
        }
#endif
        [Fact]
        public void ShouldOverwriteAComplexTypeCollection()
        {
            var source = new PublicField<PublicField<int>[]>
            {
                Value = new[] { new PublicField<int> { Value = 123 }, new PublicField<int> { Value = 456 } }
            };

            var target = new PublicField<ICollection<PublicField<int>>>
            {
                Value = new List<PublicField<int>> { new PublicField<int> { Value = 789 } }
            };

            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeSameAs(target.Value);
            result.Value.ShouldBe(r => r.Value, 123, 456);
        }

        [Fact]
        public void ShouldOverwriteAnIdentifiableComplexTypeList()
        {
            var source = new PublicProperty<Product[]>
            {
                Value = new[]
                {
                    new Product { ProductId = "Magic", Price = 0.01 },
                    new Product { ProductId = "Science", Price = 1000.00 }
                }
            };

            var target = new PublicField<IList<Product>>
            {
                Value = new List<Product> { new Product { ProductId = "Magic", Price = 1001.00 } }
            };

            var existingProduct = target.Value.First();
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeSameAs(target.Value);
            result.Value.First().ShouldBeSameAs(existingProduct);
            result.Value.First().Price.ShouldBe(0.01);
            result.Value.ShouldBe(r => r.ProductId, "Magic", "Science");
        }

        [Fact]
        public void ShouldHandleANullSourceMember()
        {
            var source = new PublicField<IEnumerable<int>> { Value = null };

            var target = new PublicProperty<ICollection<int>>
            {
                Value = new[] { 1, 2, 4, 8 }
            };

            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleNoMatchingSourceMember()
        {
            var source = new { DooDeeDoo = "Jah jah jah" };

            var target = new PublicProperty<List<long>>
            {
                Value = new List<long> { 1, 2, 3 }
            };

            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeSameAs(target.Value);
        }

        [Fact]
        public void ShouldOverwriteANonNullReadOnlyNestedHashSet()
        {
            var source = new PublicField<int[]> { Value = new[] { 1, 2, 3 } };
            var ints = new HashSet<int> { 4, 5, 6 };
            var target = new PublicReadOnlyField<HashSet<int>>(ints);
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeSameAs(ints);
            result.Value.ShouldBe(1, 2, 3);
        }

        [Fact]
        public void ShouldOverwriteANonNullReadOnlyNestedCollection()
        {
            var source = new PublicField<string[]> { Value = new[] { "One!", "Two!", "Three" } };
            var strings = new Collection<string> { "A", "B", "C" };
            var target = new PublicReadOnlyField<Collection<string>>(strings);
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeSameAs(strings);
            result.Value.ShouldBe("One!", "Two!", "Three");
        }

        [Fact]
        public void ShouldHandleAReadOnlyNestedReadOnlyCollection()
        {
            var source = new PublicField<string[]> { Value = new[] { "One!", "Two!" } };
            var strings = new List<string> { "A", "B", "C" }.AsReadOnly();
            var target = new PublicReadOnlyField<ReadOnlyCollection<string>>(strings);
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldNotBeNull();
            result.Value.ShouldBeSameAs(strings);
            result.Value.ShouldBe("A", "B", "C");
        }

#if !NET35
        [Fact]
        public void ShouldHandleANullReadOnlyNestedIReadOnlyCollection()
        {
            var source = new PublicField<string[]> { Value = new[] { "Alpha!", "Beta!" } };
            var target = new PublicReadOnlyField<IReadOnlyCollection<string>>(null);
            var result = Mapper.Map(source).Over(target);

            result.Value.ShouldBeNull();
        }
#endif
    }
}
