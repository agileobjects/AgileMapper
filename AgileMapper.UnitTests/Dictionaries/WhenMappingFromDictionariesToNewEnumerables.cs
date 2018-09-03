namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingFromDictionariesToNewEnumerables
    {
        [Fact]
        public void ShouldMapToASimpleTypeCollectionFromATypedSourceArray()
        {
            var source = new Dictionary<string, long[]> { ["Value"] = new long[] { 4, 5, 6 } };
            var result = Mapper.Map(source).ToANew<PublicProperty<ICollection<long>>>();

            result.Value.ShouldBe(4, 5, 6);
        }

        [Fact]
        public void ShouldMapToASimpleTypeListFromNullableTypedSourceEntries()
        {
            var source = new Dictionary<string, int?>
            {
                ["Value[0]"] = 56,
                ["Value[1]"] = null,
                ["Value[2]"] = 27382
            };
            var result = Mapper.Map(source).ToANew<PublicField<IList<byte?>>>();

            result.Value.ShouldBe<byte?>(56, null, default(byte?));
        }

        [Fact]
        public void ShouldMapToASimpleTypeEnumerableFromAnUntypedSourceArray()
        {
            var source = new Dictionary<string, object> { ["Value"] = new[] { 1, 2, 3 } };
            var result = Mapper.Map(source).ToANew<PublicProperty<IEnumerable<int>>>();

            result.Value.ShouldBe(1, 2, 3);
        }

        [Fact]
        public void ShouldMapToASimpleTypeArrayFromAConvertibleTypedSourceEnumerable()
        {
            var source = new Dictionary<string, IEnumerable<int>> { ["Value"] = new[] { 4, 5, 6 } };
            var result = Mapper.Map(source).ToANew<PublicProperty<string[]>>();

            result.Value.ShouldBe("4", "5", "6");
        }

        [Fact]
        public void ShouldMapToAComplexTypeArrayFromAConvertibleTypedSourceEnumerable()
        {
            var source = new Dictionary<string, IEnumerable<Person>>
            {
                ["Value"] = new[]
                {
                    new Person { Name = "Mr Pants"},
                    new Customer { Name = "Mrs Blouse" }
                }

            };
            var result = Mapper.Map(source).ToANew<PublicProperty<PersonViewModel[]>>();

            result.Value.Length.ShouldBe(2);
            result.Value.First().Name.ShouldBe("Mr Pants");
            result.Value.Second().Name.ShouldBe("Mrs Blouse");
        }

        [Fact]
        public void ShouldMapToASimpleTypeEnumerableFromTypedEntries()
        {
            var source = new Dictionary<string, int>
            {
                ["[0]"] = 1,
                ["[1]"] = 2,
                ["[2]"] = 3
            };
            var result = Mapper.Map(source).ToANew<IEnumerable<int>>();

            result.ShouldBe(1, 2, 3);
        }

        [Fact]
        public void ShouldMapToASimpleTypeArrayFromConvertibleTypedEntries()
        {
            var source = new Dictionary<string, long>
            {
                ["[0]"] = 123,
                ["[1]"] = long.MaxValue,
                ["[2]"] = 789
            };
            var result = Mapper.Map(source).ToANew<int[]>();

            result.Length.ShouldBe(3);
            result.First().ShouldBe(123);
            result.Second().ShouldBeDefault();
            result.Third().ShouldBe(789);
        }

        [Fact]
        public void ShouldMapToAComplexTypeCollectionFromTypedEntries()
        {
            var source = new Dictionary<string, MegaProduct>
            {
                ["[0]"] = new MegaProduct { ProductId = "asdfasdf" },
                ["[1]"] = new MegaProduct { ProductId = "mnbvmnbv" }
            };
            var result = Mapper.Map(source).ToANew<ICollection<MegaProduct>>();

            result.Count.ShouldBe(2);
            result.First().ShouldNotBeSameAs(source.First().Value);
            result.First().ProductId.ShouldBe("asdfasdf");
            result.Second().ShouldNotBeSameAs(source.Second().Value);
            result.Second().ProductId.ShouldBe("mnbvmnbv");
        }

        [Fact]
        public void ShouldMapToAComplexTypeListFromUntypedEntries()
        {
            var source = new Dictionary<string, object>
            {
                ["[0]"] = new Product { ProductId = "Pants" },
                ["[1]"] = new MegaProduct { ProductId = "Blouse" }
            };
            var result = Mapper.Map(source).ToANew<List<Product>>();

            result.Count.ShouldBe(2);
            result.First().ProductId.ShouldBe("Pants");
            result.Second().ShouldBeOfType<MegaProduct>();
            result.Second().ProductId.ShouldBe("Blouse");
        }

        [Fact]
        public void ShouldMapToAComplexTypeEnumerableFromTypedDottedEntries()
        {
            var source = new Dictionary<string, string>
            {
                ["[0].ProductId"] = "Hose",
                ["[0].Price"] = "1.99"
            };
            var result = Mapper.Map(source).ToANew<IEnumerable<Product>>();

            result.ShouldHaveSingleItem();
            result.First().ProductId.ShouldBe("Hose");
            result.First().Price.ShouldBe(1.99);
        }

        [Fact]
        public void ShouldMapToAParameterisedConstructorComplexTypeEnumerableFromTypedDottedEntries()
        {
            var source = new Dictionary<string, string>
            {
                ["[0].Value"] = "123",
                ["[1].Value"] = "456",
                ["[2].value"] = "789"
            };
            var result = Mapper.Map(source).ToANew<IEnumerable<PublicCtor<int>>>();

            result.Count().ShouldBe(3);
            result.First().Value.ShouldBe(123);
            result.Second().Value.ShouldBe(456);
            result.Third().Value.ShouldBe(789);
        }

        [Fact]
        public void ShouldMapToAComplexTypeCollectionFromUntypedDottedEntries()
        {
            var source = new Dictionary<string, object>
            {
                ["[0].ProductId"] = "Blouse",
                ["[0].Price"] = "10.99",
                ["[1].ProductId"] = "Pants",
                ["[1].Price"] = "7.99"
            };
            var result = Mapper.Map(source).ToANew<ICollection<Product>>();

            result.Count.ShouldBe(2);
            result.First().ProductId.ShouldBe("Blouse");
            result.First().Price.ShouldBe(10.99);
            result.Second().ProductId.ShouldBe("Pants");
            result.Second().Price.ShouldBe(7.99);
        }

        [Fact]
        public void ShouldMapToComplexTypeAndSimpleTypeArrayConstructorParametersFromUntypedDottedEntries()
        {
            var now = DateTime.Now;
            var nowString = now.ToCurrentCultureString();
            var inTenMinutes = now.AddMinutes(10).ToCurrentCultureString();
            var inTwentyMinutes = now.AddMinutes(20).ToCurrentCultureString();

            var source = new Dictionary<string, object>
            {
                ["Value1.ProductId"] = "Boom",
                ["Value1.Price"] = "1.99",
                ["Value1.HowMega"] = "1.00",
                ["Value2[0]"] = nowString,
                ["Value2[1]"] = inTenMinutes,
                ["Value2[2]"] = inTwentyMinutes
            };
            var result = Mapper.Map(source).ToANew<PublicTwoParamCtor<MegaProduct, DateTime?[]>>();

            result.Value1.ProductId.ShouldBe("Boom");
            result.Value1.Price.ShouldBe(1.99);
            result.Value1.HowMega.ShouldBe(1.00);

            result.Value2.ShouldBe(d => d.ToCurrentCultureString(), nowString, inTenMinutes, inTwentyMinutes);
        }
    }
}