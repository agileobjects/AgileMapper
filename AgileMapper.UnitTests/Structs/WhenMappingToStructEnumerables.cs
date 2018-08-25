namespace AgileObjects.AgileMapper.UnitTests.Structs
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Common;
    using TestClasses;
    using static System.Decimal;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingToStructEnumerables
    {
        [Fact]
        public void ShouldCreateAStructList()
        {
            var source = new List<PublicPropertyStruct<string>>
            {
                new PublicPropertyStruct<string> { Value = "123.5" },
                new PublicPropertyStruct<string> { Value = "456.7" }
            };

            var result = Mapper.Map(source).ToANew<List<PublicPropertyStruct<double>>>();

            result.Count.ShouldBe(2);
            result.First().Value.ShouldBe(123.5);
            result.Second().Value.ShouldBe(456.7);
        }

        [Fact]
        public void ShouldHandleANullRuntimeTypedComplexTypeElement()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Product>()
                    .ToANew<PublicPropertyStruct<string>>()
                    .Map((p, pps) => p.ProductId)
                    .To(pps => pps.Value);

                var source = new List<object>
                {
                    null,
                    new MegaProduct { ProductId = "Boomstick" }
                };

                var result = mapper.Map(source).ToANew<List<PublicPropertyStruct<string>>>();

                result.Count.ShouldBe(2);
                result.First().ShouldBeDefault();
                result.Second().Value.ShouldBe("Boomstick");
            }
        }

        [Fact]
        public void ShouldMergeARootComplexTypeArray()
        {
            var source = new[]
            {
                new PublicCtorStruct<string>("MASSIVE")
            };

            var target = new[]
            {
                new PublicPropertyStruct<string> { Value = "tiny" }
            };

            var result = Mapper.Map(source).OnTo(target);

            result.Length.ShouldBe(2);
            result.First().Value.ShouldBe("tiny");
            result.Second().Value.ShouldBe("MASSIVE");
        }

        [Fact]
        public void ShouldOverwriteANestedReadOnlyCollection()
        {
            var source = new PublicProperty<ICollection<PublicPropertyStruct<decimal>>>
            {
                Value = new[]
                {
                    new PublicPropertyStruct<decimal> { Value = MinValue },
                    new PublicPropertyStruct<decimal> { Value = MaxValue }
                }
            };

            var target = new PublicField<ReadOnlyCollection<PublicPropertyStruct<decimal>>>
            {
                Value = new ReadOnlyCollection<PublicPropertyStruct<decimal>>(new[]
                {
                    new PublicPropertyStruct<decimal> { Value = MinusOne },
                    new PublicPropertyStruct<decimal> { Value = One }
                })
            };

            var result = Mapper.Map(source).Over(target);

            result.Value.Count.ShouldBe(2);
            result.Value.First().Value.ShouldBe(MinValue);
            result.Value.Second().Value.ShouldBe(MaxValue);
        }
    }
}
